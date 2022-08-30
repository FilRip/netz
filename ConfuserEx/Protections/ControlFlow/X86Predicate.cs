using System;
using System.Collections.Generic;

using Confuser.Core;
using Confuser.Core.Services;
using Confuser.DynCipher;
using Confuser.DynCipher.AST;
using Confuser.DynCipher.Generation;
using Confuser.Renamer;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

using MethodBody = dnlib.DotNet.Writer.MethodBody;

namespace Confuser.Protections.ControlFlow
{
    internal class X86Predicate : IPredicate
    {
        static readonly object Encoding = new object();
        readonly CFContext ctx;
        X86Encoding encoding;

        bool inited;

        public X86Predicate(CFContext ctx)
        {
            this.ctx = ctx;
        }

        public void Init(CilBody body)
        {
            if (inited)
                return;

            encoding = ctx.Context.Annotations.Get<X86Encoding>(ctx.Method.DeclaringType, Encoding, null);
            if (encoding == null)
            {
                encoding = new X86Encoding();
                encoding.Compile(ctx);
                ctx.Context.Annotations.Set(ctx.Method.DeclaringType, Encoding, encoding);
            }

            inited = true;
        }

        public void EmitSwitchLoad(IList<Instruction> instrs)
        {
            instrs.Add(Instruction.Create(OpCodes.Call, encoding.native));
        }

        public int GetSwitchKey(int key)
        {
            return encoding.expCompiled(key);
        }

        class X86Encoding
        {
            byte[] code;
            MethodBody codeChunk;

            public Func<int, int> expCompiled;
            Expression expression;
            Expression inverse;
            public MethodDef native;

            public void Compile(CFContext ctx)
            {
                var var = new Variable("{VAR}");
                var result = new Variable("{RESULT}");

                CorLibTypeSig int32 = ctx.Method.Module.CorLibTypes.Int32;
                native = new MethodDefUser(ctx.Context.Registry.GetService<INameService>().RandomName(), MethodSig.CreateStatic(int32, int32), MethodAttributes.PinvokeImpl | MethodAttributes.PrivateScope | MethodAttributes.Static)
                {
                    ImplAttributes = MethodImplAttributes.Native | MethodImplAttributes.Unmanaged | MethodImplAttributes.PreserveSig
                };
                // Attempt to improve performance --- failed with StackOverflowException... :/
                //var suppressAttr = ctx.Method.Module.CorLibTypes.GetTypeRef("System.Security", "SuppressUnmanagedCodeSecurityAttribute").ResolveThrow();
                //native.CustomAttributes.Add(new CustomAttribute((MemberRef)ctx.Method.Module.Import(suppressAttr.FindDefaultConstructor())));
                //native.HasSecurity = true;
                ctx.Method.Module.GlobalType.Methods.Add(native);

                ctx.Context.Registry.GetService<IMarkerService>().Mark(native, ctx.Protection);
                ctx.Context.Registry.GetService<INameService>().SetCanRename(native, false);

                X86Register? reg;
                var codeGen = new X86CodeGen();
                do
                {
                    ctx.DynCipher.GenerateExpressionPair(
                        ctx.Random,
                        new VariableExpression { Variable = var }, new VariableExpression { Variable = result },
                        ctx.Depth, out expression, out inverse);

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
