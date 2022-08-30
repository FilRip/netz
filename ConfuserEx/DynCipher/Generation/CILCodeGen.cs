﻿using System;
using System.Collections.Generic;

using Confuser.DynCipher.AST;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.DynCipher.Generation
{
    public class CILCodeGen
    {
        readonly Dictionary<string, Local> localMap = new Dictionary<string, Local>();

        public CILCodeGen(MethodDef method, IList<Instruction> instrs)
        {
            Method = method;
            Instructions = instrs;
        }

        public MethodDef Method { get; private set; }
        public IList<Instruction> Instructions { get; private set; }

        protected void Emit(Instruction instr)
        {
            Instructions.Add(instr);
        }

        protected virtual Local Var(Variable var)
        {
            if (!localMap.TryGetValue(var.Name, out Local ret))
            {
                ret = new Local(Method.Module.CorLibTypes.UInt32)
                {
                    Name = var.Name
                };
                localMap[var.Name] = ret;
            }
            return ret;
        }

        protected virtual void LoadVar(Variable var)
        {
            Emit(Instruction.Create(OpCodes.Ldloc, Var(var)));
        }

        protected virtual void StoreVar(Variable var)
        {
            Emit(Instruction.Create(OpCodes.Stloc, Var(var)));
        }

        public void Commit(CilBody body)
        {
            foreach (Local i in localMap.Values)
            {
                body.InitLocals = true;
                body.Variables.Add(i);
            }
        }


        public void GenerateCIL(Expression expression)
        {
            EmitLoad(expression);
        }

        public void GenerateCIL(Statement statement)
        {
            EmitStatement(statement);
        }

        void EmitLoad(Expression exp)
        {
            if (exp is ArrayIndexExpression arrIndex)
            {
                EmitLoad(arrIndex.Array);
                Emit(Instruction.CreateLdcI4(arrIndex.Index));
                Emit(Instruction.Create(OpCodes.Ldelem_U4));
            }
            else if (exp is BinOpExpression binOp)
            {
                EmitLoad(binOp.Left);
                EmitLoad(binOp.Right);
                OpCode op;
                switch (binOp.Operation)
                {
                    case BinOps.Add:
                        op = OpCodes.Add;
                        break;
                    case BinOps.Sub:
                        op = OpCodes.Sub;
                        break;
                    case BinOps.Div:
                        op = OpCodes.Div;
                        break;
                    case BinOps.Mul:
                        op = OpCodes.Mul;
                        break;
                    case BinOps.Or:
                        op = OpCodes.Or;
                        break;
                    case BinOps.And:
                        op = OpCodes.And;
                        break;
                    case BinOps.Xor:
                        op = OpCodes.Xor;
                        break;
                    case BinOps.Lsh:
                        op = OpCodes.Shl;
                        break;
                    case BinOps.Rsh:
                        op = OpCodes.Shr_Un;
                        break;
                    default:
                        throw new NotSupportedException();
                }
                Emit(Instruction.Create(op));
            }
            else if (exp is UnaryOpExpression unaryOp)
            {
                EmitLoad(unaryOp.Value);
                OpCode op;
                switch (unaryOp.Operation)
                {
                    case UnaryOps.Not:
                        op = OpCodes.Not;
                        break;
                    case UnaryOps.Negate:
                        op = OpCodes.Neg;
                        break;
                    default:
                        throw new NotSupportedException();
                }
                Emit(Instruction.Create(op));
            }
            else if (exp is LiteralExpression literal)
            {
                Emit(Instruction.CreateLdcI4((int)literal.Value));
            }
            else if (exp is VariableExpression var)
            {
                LoadVar(var.Variable);
            }
            else
                throw new NotSupportedException();
        }

        void EmitStore(Expression exp, Expression value)
        {
            if (exp is ArrayIndexExpression arrIndex)
            {
                EmitLoad(arrIndex.Array);
                Emit(Instruction.CreateLdcI4(arrIndex.Index));
                EmitLoad(value);
                Emit(Instruction.Create(OpCodes.Stelem_I4));
            }
            else if (exp is VariableExpression var)
            {
                EmitLoad(value);
                StoreVar(var.Variable);
            }
            else
                throw new NotSupportedException();
        }

        void EmitStatement(Statement statement)
        {
            if (statement is AssignmentStatement assignment)
            {
                EmitStore(assignment.Target, assignment.Value);
            }
            else if (statement is LoopStatement loop)
            {
                /*
                 *      ldc.i4  begin
                 *      br      cmp
                 *      ldc.i4  dummy   //hint for dnlib
                 * lop: nop
                 *      ...
                 *      ...
                 *      ldc.i4.1
                 *      add
                 * cmp: dup
                 *      ldc.i4  limit
                 *      blt     lop
                 *      pop
                 */
                Instruction lbl = Instruction.Create(OpCodes.Nop);
                Instruction dup = Instruction.Create(OpCodes.Dup);
                Emit(Instruction.CreateLdcI4(loop.Begin));
                Emit(Instruction.Create(OpCodes.Br, dup));
                Emit(Instruction.CreateLdcI4(loop.Begin));
                Emit(lbl);

                foreach (Statement child in loop.Statements)
                    EmitStatement(child);

                Emit(Instruction.CreateLdcI4(1));
                Emit(Instruction.Create(OpCodes.Add));
                Emit(dup);
                Emit(Instruction.CreateLdcI4(loop.Limit));
                Emit(Instruction.Create(OpCodes.Blt, lbl));
                Emit(Instruction.Create(OpCodes.Pop));
            }
            else if (statement is StatementBlock block)
            {
                foreach (Statement child in block.Statements)
                    EmitStatement(child);
            }
            else
                throw new NotSupportedException();
        }
    }
}
