using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Confuser.DynCipher.AST;

namespace Confuser.DynCipher.Generation
{
    public class X86CodeGen
    {
        List<X86Instruction> instrs;
        bool[] usedRegs;

        public IList<X86Instruction> Instructions
        {
            get { return instrs; }
        }

        public int MaxUsedRegister { get; private set; }

        public X86Register? GenerateX86(Expression expression, Func<Variable, X86Register, IEnumerable<X86Instruction>> loadArg)
        {
            instrs = new List<X86Instruction>();
            usedRegs = new bool[8];
            MaxUsedRegister = -1;

            // CRITICAL registers!
            usedRegs[(int)X86Register.EBP] = true;
            usedRegs[(int)X86Register.ESP] = true;

            try
            {
                return ((X86RegisterOperand)Emit(expression, loadArg)).Register;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Register overflowed.")
                    return null;
                throw;
            }
        }

        X86Register GetFreeRegister()
        {
            for (int i = 0; i < 8; i++)
                if (!usedRegs[i])
                    return (X86Register)i;

            throw new Exception("Register overflowed.");
        }

        void TakeRegister(X86Register reg)
        {
            usedRegs[(int)reg] = true;
            if ((int)reg > MaxUsedRegister)
                MaxUsedRegister = (int)reg;
        }

        void ReleaseRegister(X86Register reg)
        {
            usedRegs[(int)reg] = false;
        }

        X86Register Normalize(X86Instruction instr)
        {
            if (instr.Operands.Length == 2 &&
                instr.Operands[0] is X86ImmediateOperand &&
                instr.Operands[1] is X86ImmediateOperand)
            {
                /*
                 * op imm1, imm2
                 * ==>
                 * mov reg, imm1
                 * op reg, imm2
                 */
                X86Register reg = GetFreeRegister();
                instrs.Add(X86Instruction.Create(X86OpCode.MOV, new X86RegisterOperand(reg), instr.Operands[0]));
                instr.Operands[0] = new X86RegisterOperand(reg);
                instrs.Add(instr);

                return reg;
            }

            if (instr.Operands.Length == 1 &&
                instr.Operands[0] is X86ImmediateOperand)
            {
                /*
                 * op imm
                 * ==>
                 * mov reg, imm
                 * op reg
                 */
                X86Register reg = GetFreeRegister();
                instrs.Add(X86Instruction.Create(X86OpCode.MOV, new X86RegisterOperand(reg), instr.Operands[0]));
                instr.Operands[0] = new X86RegisterOperand(reg);
                instrs.Add(instr);

                return reg;
            }

            if (instr.OpCode == X86OpCode.SUB &&
                instr.Operands[0] is X86ImmediateOperand &&
                instr.Operands[1] is X86RegisterOperand operand)
            {
                /*
                 * sub imm, reg
                 * ==>
                 * neg reg
                 * add reg, imm
                 */

                X86Register reg = operand.Register;
                instrs.Add(X86Instruction.Create(X86OpCode.NEG, new X86RegisterOperand(reg)));
                instr.OpCode = X86OpCode.ADD;
                instr.Operands[1] = instr.Operands[0];
                instr.Operands[0] = new X86RegisterOperand(reg);
                instrs.Add(instr);

                return reg;
            }

            if (instr.Operands.Length == 2 &&
                instr.Operands[0] is X86ImmediateOperand &&
                instr.Operands[1] is X86RegisterOperand operand1)
            {
                /*
                 * op imm, reg
                 * ==>
                 * op reg, imm
                 */

                X86Register reg = operand1.Register;
                instr.Operands[1] = instr.Operands[0];
                instr.Operands[0] = new X86RegisterOperand(reg);
                instrs.Add(instr);

                return reg;
            }
            Debug.Assert(instr.Operands.Length > 0);
            Debug.Assert(instr.Operands[0] is X86RegisterOperand);

            if (instr.Operands.Length == 2 && instr.Operands[1] is X86RegisterOperand operand2)
                ReleaseRegister(operand2.Register);

            instrs.Add(instr);

            return ((X86RegisterOperand)instr.Operands[0]).Register;
        }

        IX86Operand Emit(Expression exp, Func<Variable, X86Register, IEnumerable<X86Instruction>> loadArg)
        {
            if (exp is BinOpExpression binOp)
            {
                X86Register reg;
                switch (binOp.Operation)
                {
                    case BinOps.Add:
                        reg = Normalize(X86Instruction.Create(X86OpCode.ADD, Emit(binOp.Left, loadArg), Emit(binOp.Right, loadArg)));
                        break;

                    case BinOps.Sub:
                        reg = Normalize(X86Instruction.Create(X86OpCode.SUB, Emit(binOp.Left, loadArg), Emit(binOp.Right, loadArg)));
                        break;

                    case BinOps.Mul:
                        reg = Normalize(X86Instruction.Create(X86OpCode.IMUL, Emit(binOp.Left, loadArg), Emit(binOp.Right, loadArg)));
                        break;

                    case BinOps.Xor:
                        reg = Normalize(X86Instruction.Create(X86OpCode.XOR, Emit(binOp.Left, loadArg), Emit(binOp.Right, loadArg)));
                        break;

                    default:
                        throw new NotSupportedException();
                }
                TakeRegister(reg);
                return new X86RegisterOperand(reg);
            }

            if (exp is UnaryOpExpression unaryOp)
            {
                X86Register reg;
                switch (unaryOp.Operation)
                {
                    case UnaryOps.Negate:
                        reg = Normalize(X86Instruction.Create(X86OpCode.NEG, Emit(unaryOp.Value, loadArg)));
                        break;

                    case UnaryOps.Not:
                        reg = Normalize(X86Instruction.Create(X86OpCode.NOT, Emit(unaryOp.Value, loadArg)));
                        break;

                    default:
                        throw new NotSupportedException();
                }
                TakeRegister(reg);
                return new X86RegisterOperand(reg);
            }

            if (exp is LiteralExpression expression)
                return new X86ImmediateOperand((int)expression.Value);

            if (exp is VariableExpression expression1)
            {
                X86Register reg = GetFreeRegister();
                TakeRegister(reg);
                instrs.AddRange(loadArg(expression1.Variable, reg));
                return new X86RegisterOperand(reg);
            }

            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return string.Join("\r\n", instrs.Select(instr => instr.ToString()).ToArray());
        }
    }

    public enum X86OpCode
    {
        MOV,
        ADD,
        SUB,
        IMUL,
        DIV,
        NEG,
        NOT,
        XOR,
        POP
    }

    public enum X86Register
    {
        EAX,
        ECX,
        EDX,
        EBX,
        ESP,
        EBP,
        ESI,
        EDI
    }

    public interface IX86Operand { }

    public class X86RegisterOperand : IX86Operand
    {
        public X86RegisterOperand(X86Register reg)
        {
            Register = reg;
        }

        public X86Register Register { get; set; }

        public override string ToString()
        {
            return Register.ToString();
        }
    }

    public class X86ImmediateOperand : IX86Operand
    {
        public X86ImmediateOperand(int imm)
        {
            Immediate = imm;
        }

        public int Immediate { get; set; }

        public override string ToString()
        {
            return Immediate.ToString("X") + "h";
        }
    }

    public class X86Instruction
    {
        public X86OpCode OpCode { get; set; }
        public IX86Operand[] Operands { get; set; }

        public static X86Instruction Create(X86OpCode opCode, params IX86Operand[] operands)
        {
            var ret = new X86Instruction()
            {
                OpCode = opCode,
                Operands = operands
            };
            return ret;
        }

        public byte[] Assemble()
        {
            switch (OpCode)
            {
                case X86OpCode.MOV:
                    {
                        if (Operands.Length != 2) throw new InvalidOperationException();
                        if (Operands[0] is X86RegisterOperand &&
                            Operands[1] is X86RegisterOperand)
                        {
                            var ret = new byte[2];
                            ret[0] = 0x89;
                            ret[1] = 0xc0;
                            ret[1] |= (byte)((int)(Operands[1] as X86RegisterOperand).Register << 3);
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            return ret;
                        }
                        if (Operands[0] is X86RegisterOperand &&
                            Operands[1] is X86ImmediateOperand)
                        {
                            var ret = new byte[5];
                            ret[0] = 0xb8;
                            ret[0] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            Buffer.BlockCopy(BitConverter.GetBytes((Operands[1] as X86ImmediateOperand).Immediate), 0, ret, 1, 4);
                            return ret;
                        }
                        throw new NotSupportedException();
                    }

                case X86OpCode.ADD:
                    {
                        if (Operands.Length != 2) throw new InvalidOperationException();
                        if (Operands[0] is X86RegisterOperand &&
                            Operands[1] is X86RegisterOperand)
                        {
                            var ret = new byte[2];
                            ret[0] = 0x01;
                            ret[1] = 0xc0;
                            ret[1] |= (byte)((int)(Operands[1] as X86RegisterOperand).Register << 3);
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            return ret;
                        }
                        if (Operands[0] is X86RegisterOperand &&
                            Operands[1] is X86ImmediateOperand)
                        {
                            var ret = new byte[6];
                            ret[0] = 0x81;
                            ret[1] = 0xc0;
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            Buffer.BlockCopy(BitConverter.GetBytes((Operands[1] as X86ImmediateOperand).Immediate), 0, ret, 2, 4);
                            return ret;
                        }
                        throw new NotSupportedException();
                    }

                case X86OpCode.SUB:
                    {
                        if (Operands.Length != 2) throw new InvalidOperationException();
                        if (Operands[0] is X86RegisterOperand &&
                            Operands[1] is X86RegisterOperand)
                        {
                            var ret = new byte[2];
                            ret[0] = 0x29;
                            ret[1] = 0xc0;
                            ret[1] |= (byte)((int)(Operands[1] as X86RegisterOperand).Register << 3);
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            return ret;
                        }
                        if (Operands[0] is X86RegisterOperand &&
                            Operands[1] is X86ImmediateOperand)
                        {
                            var ret = new byte[6];
                            ret[0] = 0x81;
                            ret[1] = 0xe8;
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            Buffer.BlockCopy(BitConverter.GetBytes((Operands[1] as X86ImmediateOperand).Immediate), 0, ret, 2, 4);
                            return ret;
                        }
                        throw new NotSupportedException();
                    }

                case X86OpCode.NEG:
                    {
                        if (Operands.Length != 1) throw new InvalidOperationException();
                        if (Operands[0] is X86RegisterOperand)
                        {
                            var ret = new byte[2];
                            ret[0] = 0xf7;
                            ret[1] = 0xd8;
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            return ret;
                        }
                        throw new NotSupportedException();
                    }

                case X86OpCode.NOT:
                    {
                        if (Operands.Length != 1) throw new InvalidOperationException();
                        if (Operands[0] is X86RegisterOperand)
                        {
                            var ret = new byte[2];
                            ret[0] = 0xf7;
                            ret[1] = 0xd0;
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            return ret;
                        }
                        throw new NotSupportedException();
                    }

                case X86OpCode.XOR:
                    {
                        if (Operands.Length != 2) throw new InvalidOperationException();
                        if (Operands[0] is X86RegisterOperand &&
                            Operands[1] is X86RegisterOperand)
                        {
                            var ret = new byte[2];
                            ret[0] = 0x31;
                            ret[1] = 0xc0;
                            ret[1] |= (byte)((int)(Operands[1] as X86RegisterOperand).Register << 3);
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            return ret;
                        }
                        if (Operands[0] is X86RegisterOperand &&
                            Operands[1] is X86ImmediateOperand)
                        {
                            var ret = new byte[6];
                            ret[0] = 0x81;
                            ret[1] = 0xf0;
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            Buffer.BlockCopy(BitConverter.GetBytes((Operands[1] as X86ImmediateOperand).Immediate), 0, ret, 2, 4);
                            return ret;
                        }
                        throw new NotSupportedException();
                    }

                case X86OpCode.POP:
                    {
                        if (Operands.Length != 1) throw new InvalidOperationException();
                        if (Operands[0] is X86RegisterOperand)
                        {
                            var ret = new byte[1];
                            ret[0] = 0x58;
                            ret[0] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            return ret;
                        }
                        throw new NotSupportedException();
                    }

                case X86OpCode.IMUL:
                    {
                        if (Operands.Length != 2) throw new InvalidOperationException();
                        if (Operands[0] is X86RegisterOperand &&
                            Operands[1] is X86RegisterOperand)
                        {
                            var ret = new byte[3];
                            ret[0] = 0x0f;
                            ret[1] = 0xaf;
                            ret[1] = 0xc0;
                            ret[1] |= (byte)((int)(Operands[1] as X86RegisterOperand).Register << 3);
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            return ret;
                        }
                        if (Operands[0] is X86RegisterOperand &&
                            Operands[1] is X86ImmediateOperand)
                        {
                            var ret = new byte[6];
                            ret[0] = 0x69;
                            ret[1] = 0xc0;
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 3);
                            ret[1] |= (byte)((int)(Operands[0] as X86RegisterOperand).Register << 0);
                            Buffer.BlockCopy(BitConverter.GetBytes((Operands[1] as X86ImmediateOperand).Immediate), 0, ret, 2, 4);
                            return ret;
                        }
                        throw new NotSupportedException();
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        public override string ToString()
        {
            var ret = new StringBuilder();
            ret.Append(OpCode);
            for (int i = 0; i < Operands.Length; i++)
            {
                ret.AppendFormat("{0}{1}", i == 0 ? " " : ", ", Operands[i]);
            }
            return ret.ToString();
        }
    }
}
