﻿using System;
using System.Collections.Generic;

using Confuser.Core.Services;
using Confuser.DynCipher;
using Confuser.DynCipher.AST;
using Confuser.DynCipher.Generation;
using Confuser.Renamer;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

using MethodBody = dnlib.DotNet.Writer.MethodBody;

namespace Confuser.Protections.ReferenceProxy
{
    internal class X86Encoding : IRPEncoding
    {
        readonly Dictionary<MethodDef, Tuple<MethodDef, Func<int, int>>> keys = new Dictionary<MethodDef, Tuple<MethodDef, Func<int, int>>>();
        readonly List<Tuple<MethodDef, byte[], MethodBody>> nativeCodes = new List<Tuple<MethodDef, byte[], MethodBody>>();
        bool addedHandler;

        public Instruction[] EmitDecode(MethodDef init, RPContext ctx, Instruction[] arg)
        {
            Tuple<MethodDef, Func<int, int>> key = GetKey(ctx, init);

            var repl = new List<Instruction>();
            repl.AddRange(arg);
            repl.Add(Instruction.Create(OpCodes.Call, key.Item1));
            return repl.ToArray();
        }

        public int Encode(MethodDef init, RPContext ctx, int value)
        {
            Tuple<MethodDef, Func<int, int>> key = GetKey(ctx, init);
            return key.Item2(value);
        }

        void Compile(RPContext ctx, out Func<int, int> expCompiled, out MethodDef native)
        {
            var var = new Variable("{VAR}");
            var result = new Variable("{RESULT}");

            CorLibTypeSig int32 = ctx.Module.CorLibTypes.Int32;
            native = new MethodDefUser(ctx.Context.Registry.GetService<INameService>().RandomName(), MethodSig.CreateStatic(int32, int32), MethodAttributes.PinvokeImpl | MethodAttributes.PrivateScope | MethodAttributes.Static)
            {
                ImplAttributes = MethodImplAttributes.Native | MethodImplAttributes.Unmanaged | MethodImplAttributes.PreserveSig
            };
            ctx.Module.GlobalType.Methods.Add(native);

            ctx.Context.Registry.GetService<IMarkerService>().Mark(native, ctx.Protection);
            ctx.Context.Registry.GetService<INameService>().SetCanRename(native, false);

            X86Register? reg;
            var codeGen = new X86CodeGen();
            Expression expression;
            do
            {
                ctx.DynCipher.GenerateExpressionPair(
                    ctx.Random,
                    new VariableExpression { Variable = var }, new VariableExpression { Variable = result },
                    ctx.Depth, out expression, out Expression inverse);

                reg = codeGen.GenerateX86(inverse, (v, r) => { return new[] { X86Instruction.Create(X86OpCode.POP, new X86RegisterOperand(r)) }; });
            } while (reg == null);

            byte[] code = CodeGenUtils.AssembleCode(codeGen, reg.Value);

            expCompiled = new DMCodeGen(typeof(int), new[] { Tuple.Create("{VAR}", typeof(int)) })
                .GenerateCIL(expression)
                .Compile<Func<int, int>>();

            nativeCodes.Add(Tuple.Create(native, code, (MethodBody)null));
            if (!addedHandler)
            {
                ctx.Context.CurrentModuleWriterOptions.WriterEvent += InjectNativeCode;
                addedHandler = true;
            }
        }

        void InjectNativeCode(object sender, ModuleWriterEventArgs e)
        {
            var writer = (ModuleWriterBase)sender;
            if (e.Event == ModuleWriterEvent.MDEndWriteMethodBodies)
            {
                for (int n = 0; n < nativeCodes.Count; n++)
                    nativeCodes[n] = new Tuple<MethodDef, byte[], MethodBody>(
                        nativeCodes[n].Item1,
                        nativeCodes[n].Item2,
                        writer.MethodBodies.Add(new MethodBody(nativeCodes[n].Item2)));
            }
            else if (e.Event == ModuleWriterEvent.EndCalculateRvasAndFileOffsets)
            {
                foreach (var native in nativeCodes)
                {
                    uint rid = writer.Metadata.GetRid(native.Item1);
                    dnlib.DotNet.MD.RawMethodRow row = writer.Metadata.TablesHeap.MethodTable[rid];
                    writer.Metadata.TablesHeap.MethodTable[rid] = new dnlib.DotNet.MD.RawMethodRow((uint)native.Item3.RVA, row.ImplFlags, row.Flags, row.Name, row.Signature, row.ParamList);
                }
            }
        }

        Tuple<MethodDef, Func<int, int>> GetKey(RPContext ctx, MethodDef init)
        {
            if (!keys.TryGetValue(init, out Tuple<MethodDef, Func<int, int>> ret))
            {
                Compile(ctx, out Func<int, int> keyFunc, out MethodDef native);
                keys[init] = ret = Tuple.Create(native, keyFunc);
            }
            return ret;
        }

        class CodeGen : CILCodeGen
        {
            readonly Instruction[] arg;

            public CodeGen(Instruction[] arg, MethodDef method, IList<Instruction> instrs)
                : base(method, instrs)
            {
                this.arg = arg;
            }

            protected override void LoadVar(Variable var)
            {
                if (var.Name == "{RESULT}")
                {
                    foreach (Instruction instr in arg)
                        Emit(instr);
                }
                else
                    base.LoadVar(var);
            }
        }
    }
}
