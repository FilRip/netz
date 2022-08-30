using Confuser.DynCipher.AST;

namespace Confuser.DynCipher.Transforms
{
    internal class ConvertVariables
    {
        static Expression ReplaceVar(Expression exp, Variable buff)
        {
            if (exp is VariableExpression expression1)
            {
                if (expression1.Variable.Name[0] != 'v') return exp;
                return new ArrayIndexExpression
                {
                    Array = new VariableExpression { Variable = buff },
                    Index = (int)(exp as VariableExpression).Variable.Tag
                };
            }
            if (exp is ArrayIndexExpression expression2)
            {
                expression2.Array = ReplaceVar(expression2.Array, buff);
            }
            else if (exp is BinOpExpression expression3)
            {
                expression3.Left = ReplaceVar(expression3.Left, buff);
                expression3.Right = ReplaceVar(expression3.Right, buff);
            }
            else if (exp is UnaryOpExpression expression4)
            {
                expression4.Value = ReplaceVar(expression4.Value, buff);
            }
            return exp;
        }

        static Statement ReplaceVar(Statement st, Variable buff)
        {
            if (st is AssignmentStatement statement)
            {
                statement.Value = ReplaceVar(statement.Value, buff);
                statement.Target = ReplaceVar(statement.Target, buff);
            }
            return st;
        }

        public static void Run(StatementBlock block)
        {
            var mainBuff = new Variable("{BUFFER}");
            for (int i = 0; i < block.Statements.Count; i++)
                block.Statements[i] = ReplaceVar(block.Statements[i], mainBuff);
        }
    }
}
