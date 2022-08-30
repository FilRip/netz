﻿using System;
using System.Collections.Generic;

using Confuser.Core.Project.Patterns;

namespace Confuser.Core.Project
{
    /// <summary>
    ///     Parser of pattern expressions.
    /// </summary>
    public class PatternParser
    {
        static readonly Dictionary<string, Func<PatternFunction>> fns;
        static readonly Dictionary<string, Func<PatternOperator>> ops;
        readonly PatternTokenizer tokenizer = new PatternTokenizer();
        PatternToken? lookAhead;

        static PatternParser()
        {
            fns = new Dictionary<string, Func<PatternFunction>>(StringComparer.OrdinalIgnoreCase)
            {
                { ModuleFunction.FnName, () => new ModuleFunction() },
                { DeclTypeFunction.FnName, () => new DeclTypeFunction() },
                { NamespaceFunction.FnName, () => new NamespaceFunction() },
                { NameFunction.FnName, () => new NameFunction() },
                { FullNameFunction.FnName, () => new FullNameFunction() },
                { MatchFunction.FnName, () => new MatchFunction() },
                { MatchNameFunction.FnName, () => new MatchNameFunction() },
                { MatchTypeNameFunction.FnName, () => new MatchTypeNameFunction() },
                { MemberTypeFunction.FnName, () => new MemberTypeFunction() },
                { IsPublicFunction.FnName, () => new IsPublicFunction() },
                { InheritsFunction.FnName, () => new InheritsFunction() },
                { IsTypeFunction.FnName, () => new IsTypeFunction() },
                { HasAttrFunction.FnName, () => new HasAttrFunction() }
            };

            ops = new Dictionary<string, Func<PatternOperator>>(StringComparer.OrdinalIgnoreCase)
            {
                { AndOperator.OpName, () => new AndOperator() },
                { OrOperator.OpName, () => new OrOperator() },
                { NotOperator.OpName, () => new NotOperator() }
            };
        }

        /// <summary>
        ///     Parses the specified pattern into expression.
        /// </summary>
        /// <param name="pattern">The pattern to parse.</param>
        /// <returns>The parsed expression.</returns>
        /// <exception cref="InvalidPatternException">
        ///     The pattern is invalid.
        /// </exception>
        public PatternExpression Parse(string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");

            try
            {
                tokenizer.Initialize(pattern);
                lookAhead = tokenizer.NextToken();
                PatternExpression ret = ParseExpression(true);
                if (PeekToken() != null)
                    throw new InvalidPatternException("Extra tokens beyond the end of pattern.");
                return ret;
            }
            catch (Exception ex)
            {
                if (ex is InvalidPatternException)
                    throw;
                throw new InvalidPatternException("Invalid pattern.", ex);
            }
        }

        static bool IsFunction(PatternToken token)
        {
            if (token.Type != TokenType.Identifier)
                return false;
            return fns.ContainsKey(token.Value);
        }

        static bool IsOperator(PatternToken token)
        {
            if (token.Type != TokenType.Identifier)
                return false;
            return ops.ContainsKey(token.Value);
        }

        Exception UnexpectedEnd()
        {
            throw new InvalidPatternException("Unexpected end of pattern.");
        }

        Exception MismatchParens(int position)
        {
            throw new InvalidPatternException(string.Format("Mismatched parentheses at position {0}.", position));
        }

        Exception UnknownToken(PatternToken token)
        {
            throw new InvalidPatternException(string.Format("Unknown token '{0}' at position {1}.", token.Value, token.Position));
        }

        Exception UnexpectedToken(PatternToken token)
        {
            throw new InvalidPatternException(string.Format("Unexpected token '{0}' at position {1}.", token.Value, token.Position));
        }

        Exception UnexpectedToken(PatternToken token, char expect)
        {
            throw new InvalidPatternException(string.Format("Unexpected token '{0}' at position {1}. Expected '{2}'.", token.Value, token.Position, expect));
        }

        Exception BadArgCount(PatternToken token, int expected)
        {
            throw new InvalidPatternException(string.Format("Invalid argument count for '{0}' at position {1}. Expected {2}", token.Value, token.Position, expected));
        }

        PatternToken ReadToken()
        {
            if (lookAhead == null)
                throw UnexpectedEnd();
            PatternToken ret = lookAhead.Value;
            lookAhead = tokenizer.NextToken();
            return ret;
        }

        PatternToken? PeekToken()
        {
            return lookAhead;
        }

        PatternExpression ParseExpression(bool readBinOp = false)
        {
            PatternExpression ret;
            PatternToken token = ReadToken();
            switch (token.Type)
            {
                case TokenType.Literal:
                    ret = new LiteralExpression(token.Value);
                    break;
                case TokenType.LParens:
                    {
                        ret = ParseExpression(true);
                        PatternToken parens = ReadToken();
                        if (parens.Type != TokenType.RParens)
                            throw MismatchParens(token.Position.Value);
                    }
                    break;
                case TokenType.Identifier:
                    if (IsOperator(token))
                    {
                        // unary operator
                        PatternOperator op = ops[token.Value]();
                        if (!op.IsUnary)
                            throw UnexpectedToken(token);
                        op.OperandA = ParseExpression();
                        ret = op;
                    }
                    else if (IsFunction(token))
                    {
                        // function
                        PatternFunction fn = fns[token.Value]();

                        PatternToken parens = ReadToken();
                        if (parens.Type != TokenType.LParens)
                            throw UnexpectedToken(parens, '(');

                        fn.Arguments = new List<PatternExpression>(fn.ArgumentCount);
                        for (int i = 0; i < fn.ArgumentCount; i++)
                        {
                            if (PeekToken() == null)
                                throw UnexpectedEnd();
                            if (PeekToken().Value.Type == TokenType.RParens)
                                throw BadArgCount(token, fn.ArgumentCount);
                            if (i != 0)
                            {
                                PatternToken comma = ReadToken();
                                if (comma.Type != TokenType.Comma)
                                    throw UnexpectedToken(comma, ',');
                            }
                            fn.Arguments.Add(ParseExpression());
                        }

                        parens = ReadToken();
                        if (parens.Type == TokenType.Comma)
                            throw BadArgCount(token, fn.ArgumentCount);
                        if (parens.Type != TokenType.RParens)
                            throw MismatchParens(parens.Position.Value);

                        ret = fn;
                    }
                    else
                    {
                        if (bool.TryParse(token.Value, out bool boolValue))
                            ret = new LiteralExpression(boolValue);
                        else
                            throw UnknownToken(token);
                    }

                    break;
                default:
                    throw UnexpectedToken(token);
            }

            if (!readBinOp)
                return ret;

            // binary operator
            PatternToken? peek = PeekToken();
            while (peek != null)
            {
                if (peek.Value.Type != TokenType.Identifier)
                    break;
                if (!IsOperator(peek.Value))
                    break;

                PatternToken binOpToken = ReadToken();
                PatternOperator binOp = ops[binOpToken.Value]();
                if (binOp.IsUnary)
                    throw UnexpectedToken(binOpToken);
                binOp.OperandA = ret;
                binOp.OperandB = ParseExpression();
                ret = binOp;

                peek = PeekToken();
            }

            return ret;
        }
    }
}
