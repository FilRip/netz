using Confuser.DynCipher.AST;

namespace Confuser.DynCipher.Transforms
{
    internal class NormalizeBinOpTransform
    {
        static Expression ProcessExpression(Expression exp)
        {
            if (exp is BinOpExpression binOp)
            {
                //  a + (b + c) => (a + b) + c
                if (binOp.Right is BinOpExpression binOpRight && binOpRight.Operation == binOp.Operation &&
                    (binOp.Operation == BinOps.Add || binOp.Operation == BinOps.Mul ||
                     binOp.Operation == BinOps.Or || binOp.Operation == BinOps.And ||
                     binOp.Operation == BinOps.Xor))
                {
                    binOp.Left = new BinOpExpression
                    {
                        Left = binOp.Left,
                        Operation = binOp.Operation,
                        Right = binOpRight.Left
                    };
                    binOp.Right = binOpRight.Right;
                }

                binOp.Left = ProcessExpression(binOp.Left);
                binOp.Right = ProcessExpression(binOp.Right);

                if (binOp.Right is LiteralExpression expression && expression.Value == 0 &&
                    binOp.Operation == BinOps.Add) // x + 0 => x
                    return binOp.Left;
            }
            else if (exp is ArrayIndexExpression expression)
            {
                expression.Array = ProcessExpression(expression.Array);
            }
            else if (exp is UnaryOpExpression expression2)
            {
                expression2.Value = ProcessExpression(expression2.Value);
            }
            return exp;
        }

        static void ProcessStatement(Statement st)
        {
            if (st is AssignmentStatement assign)
            {
                assign.Target = ProcessExpression(assign.Target);
                assign.Value = ProcessExpression(assign.Value);
            }
        }

        public static void Run(StatementBlock block)
        {
            foreach (Statement st in block.Statements)
                ProcessStatement(st);
        }
    }
}
