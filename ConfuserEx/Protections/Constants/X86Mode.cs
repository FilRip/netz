﻿using System;
using System.Collections.Generic;

using Confuser.Core.Helpers;
using Confuser.DynCipher;
using Confuser.DynCipher.AST;
using Confuser.DynCipher.Generation;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

using MethodBody = dnlib.DotNet.Writer.MethodBody;

namespace Confuser.Protections.Constants
{
    internal class X86Mode : IEncodeMode
    {
        Action<uint[], uint[]> encryptFunc;

        public IEnumerable<Instruction> EmitDecrypt(MethodDef init, CEContext ctx, Local block, Local key)
        {
            ctx.DynCipher.GenerateCipherPair(ctx.Random, out StatementBlock encrypt, out StatementBlock decrypt);
            var ret = new List<Instruction>();

            var codeGen = new CipherCodeGen(block, key, init, ret);
            codeGen.GenerateCIL(decrypt);
            codeGen.Commit(init.Body);

            var dmCodeGen = new DMCodeGen(typeof(void), new[] {
                Tuple.Create("{BUFFER}", typeof(uint[])),
                Tuple.Create("{KEY}", typeof(uint[]))
            });
            dmCodeGen.GenerateCIL(encrypt);
            encryptFunc = dmCodeGen.Compile<Action<uint[], uint[]>>();

            return ret;
        }

        public uint[] Encrypt(uint[] data, int offset, uint[] key)
        {
            var ret = new uint[key.Length];
            Buffer.BlockCopy(data, offset * sizeof(uint), ret, 0, key.Length * sizeof(uint));
            encryptFunc(ret, key);
            return ret;
        }

        public object CreateDecoder(MethodDef decoder, CEContext ctx)
        {
            var encoding = new X86Encoding();
            encoding.Compile(ctx);
            MutationHelper.ReplacePlaceholder(decoder, arg =>
            {
                var repl = new List<Instruction>();
                repl.AddRange(arg);
                repl.Add(Instruction.Create(OpCodes.Call, encoding.native));
                return repl.ToArray();
            });
            return encoding;
        }

        public uint Encode(object data, CEContext ctx, uint id)
        {
            var encoding = (X86Encoding)data;
            return (uint)encoding.expCompiled((int)id);
        }

        class CipherCodeGen : CILCodeGen
        {
            readonly Local block;
            readonly Local key;

            public CipherCodeGen(Local block, Local key, MethodDef init, IList<Instruction> instrs)
                : base(init, instrs)
            {
                this.block = block;
                this.key = key;
            }

            protected override Local Var(Variable var)
            {
                if (var.Name == "{BUFFER}")
                    return block;
                if (var.Name == "{KEY}")
                    return key;
                return base.Var(var);
            }
        }

        class X86Encoding
        {
            byte[] code;
            MethodBody codeChunk;

            public Func<int, int> expCompiled;
            Expression expression;
            Expression inverse;
            public MethodDef native;

            public void Compile(CEContext ctx)
            {
                var var = new Variable("{VAR}");
                var result = new Variable("{RESULT}");

                CorLibTypeSig int32 = ctx.Module.CorLibTypes.Int32;
                native = new MethodDefUser("", MethodSig.CreateStatic(int32, int32), MethodAttributes.PinvokeImpl | MethodAttributes.PrivateScope | MethodAttributes.Static)
                {
                    ImplAttributes = MethodImplAttributes.Native | MethodImplAttributes.Unmanaged | MethodImplAttributes.PreserveSig
                };
                // Attempt to improve performance --- failed with StackOverflowException... :/
                //var suppressAttr = ctx.Method.Module.CorLibTypes.GetTypeRef("System.Security", "SuppressUnmanagedCodeSecurityAttribute").ResolveThrow();
                //native.CustomAttributes.Add(new CustomAttribute((MemberRef)ctx.Method.Module.Import(suppressAttr.FindDefaultConstructor())));
                //native.HasSecurity = true;
                ctx.Module.GlobalType.Methods.Add(native);

                ctx.Name.MarkHelper(native, ctx.Marker, ctx.Protection);

                X86Register? reg;
                var codeGen = new X86CodeGen();
                do
                {
                    ctx.DynCipher.GenerateExpressionPair(
                        ctx.Random,
                        new VariableExpression { Variable = var }, new VariableExpression { Variable = result },
                        4, out expression, out inverse);

                    reg = codeGen.GenerateX86(inverse, (v, r) => { return new[] { X86Instruction.Create(X86OpCode.POP, new X86RegisterOperand(r)) }; });
                } while (reg == null);

                code = CodeGenUtils.AssembleCode(codeGen, reg.Value);

                expCompiled = new DMCodeGen(typeof(int), new[] { Tuple.Create("{VAR}", typeof(int)) })
                    .GenerateCIL(expression)
                    .Compile<Func<int, int>>();


                ctx.Context.CurrentModuleWriterOptions.WriterEvent += InjectNativeCode;
            }

            void InjectNativeCode(object sender, ModuleWriterEventArgs e)
            {
                var writer = (ModuleWriterBase)sender;
                if (e.Event == ModuleWriterEvent.MDEndWriteMethodBodies)
                {
                    codeChunk = writer.MethodBodies.Add(new MethodBody(code));
                }
                else if (e.Event == ModuleWriterEvent.EndCalculateRvasAndFileOffsets)
                {
                    uint rid = writer.Metadata.GetRid(native);
                    dnlib.DotNet.MD.RawMethodRow raw = writer.Metadata.TablesHeap.MethodTable[rid];
                    writer.Metadata.TablesHeap.MethodTable[rid] = new dnlib.DotNet.MD.RawMethodRow((uint)codeChunk.RVA, raw.ImplFlags, raw.Flags, raw.Name, raw.Signature, raw.ParamList);
                }
            }
        }
    }
}
