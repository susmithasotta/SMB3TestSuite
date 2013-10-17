using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public enum ScannerState
    {
        Space,
        Number,
        Identifier,
        Operator,
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public class ExpressionLexer
    {
        private IEnumerator<IToken> tokens;

        private IToken token;

        public ExpressionLexer(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            this.tokens = Tokenize(expression);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IToken GetNextToken()
        {
            if (tokens.MoveNext())
            {
                token = tokens.Current;
                if (token.Type == TokenType.Invalid)
                {
                    throw new ExpressionEvaluatorException(
                        String.Format("invalid token '{0}'", token.Text.Substring(0)));
                }

                return token;
            }
            else
                return new Token(TokenType.EndOfFile, String.Empty);
        }

        protected IEnumerator<IToken> Tokenize(string expression)
        {
            int pos = 0;
            int n = expression.Length;
            StringBuilder currentToken = new StringBuilder();
            ScannerState state = ScannerState.Space;
            while (pos < n)
            {
                char nextCh = expression[pos++];
                switch (state)
                {
                    case ScannerState.Space:
                        if (IsWhitespace(nextCh))
                            continue;
                        if (IsIdentifier(nextCh, true))
                        {
                            currentToken.Append(nextCh);
                            state = ScannerState.Identifier;
                            continue;
                        }
                        if (IsNumber(nextCh, true))
                        {
                            currentToken.Append(nextCh);
                            state = ScannerState.Number;
                            continue;
                        }
                        if (IsOperator(nextCh))
                        {
                            currentToken.Append(nextCh);
                            state = ScannerState.Operator;
                            continue;
                        }
                        if (IsSeparator(nextCh))
                        {
                            yield return MakeSeparatorToken(nextCh.ToString());
                            continue;
                        }
                        yield return MakeInvalidToken("#" + nextCh);
                        continue;

                    case ScannerState.Identifier:
                        if (IsIdentifier(nextCh, false))
                        {
                            currentToken.Append(nextCh);
                            continue;
                        }
                        else
                        {
                            yield return MakeIdentifierToken(currentToken.ToString());
                            currentToken = new StringBuilder();
                            pos--;
                            state = ScannerState.Space;
                            continue;
                        }

                    case ScannerState.Number:
                        if (IsNumber(nextCh, false))
                        {
                            currentToken.Append(nextCh);
                            continue;
                        }
                        else
                        {
                            yield return MakeNumberToken(currentToken.ToString());
                            currentToken = new StringBuilder();
                            pos--;
                            state = ScannerState.Space;
                            continue;
                        }

                    case ScannerState.Operator:
                        if (IsOperator(nextCh)
                            && IsSupportedOperator(currentToken.ToString() + nextCh))
                        {
                            currentToken.Append(nextCh);
                            continue;
                        }
                        else
                        {
                            yield return MakeOperatorToken(currentToken.ToString());
                            currentToken = new StringBuilder();
                            pos--;
                            state = ScannerState.Space;
                            continue;
                        }
                }
            }
            if (state != ScannerState.Space)
                yield return MakeTokenFromState(currentToken.ToString(), state);
        }

        protected static bool IsIdentifier(char ch, bool start)
        {
            return (start ? Char.IsLetter(ch) : Char.IsLetterOrDigit(ch))
                || ch == '@'
                || ch == '_';
        }

        protected static bool IsNumber(char ch, bool start)
        {
            return Char.IsDigit(ch) ||
                    !start &&
                    (ch == 'x'
                    || ch == 'X'
                    || ch >= 'a' && ch <= 'f'
                    || ch >= 'A' && ch <= 'F');
        }

        protected static bool IsWhitespace(char ch)
        {
            return Char.IsWhiteSpace(ch);
        }

        protected static bool IsOperator(char ch)
        {
            string opchs = "?:&|^=!><+-*/%*~";
            return opchs.IndexOf(ch) >= 0;
        }

        protected static bool IsSeparator(char ch)
        {
            return ch == '(' || ch == ')' || ch == ',';
        }

        private static string[] supportedOperators = {
                                                           "+",
                                                           "-",
                                                           "*",
                                                           "/",
                                                           "%",
                                                           "~",
                                                           "!",
                                                           "<<",
                                                           ">>",
                                                           ">",
                                                           "<",
                                                           ">=",
                                                           "<=",
                                                           "==",
                                                           "!=",
                                                           "&&",
                                                           "||",
                                                           "&",
                                                           "|",
                                                           "^",
                                                           "?",
                                                           ":",
                                                       };
        protected static bool IsSupportedOperator(string op)
        {
            foreach (string supportedOp in supportedOperators)
            {
                if (supportedOp == op)
                {
                    return true;
                }
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected static IToken MakeOperatorToken(string op)
        {
            switch (op)
            {
                case "+":
                    return new Token(TokenType.Plus, op);
                case "-":
                    return new Token(TokenType.Minus, op);
                case "*":
                    return new Token(TokenType.Multiply, op);
                case "/":
                    return new Token(TokenType.Divide, op);
                case "%":
                    return new Token(TokenType.Multiply, op);
                case "~":
                    return new Token(TokenType.BitNot, op);
                case "!":
                    return new Token(TokenType.Not, op);
                case "<<":
                    return new Token(TokenType.ShiftLeft, op);
                case ">>":
                    return new Token(TokenType.ShiftRight, op);
                case ">":
                    return new Token(TokenType.Greater, op);
                case "<":
                    return new Token(TokenType.Lesser, op);
                case ">=":
                    return new Token(TokenType.GreaterOrEqual, op);
                case "<=":
                    return new Token(TokenType.LesserOrEqual, op);
                case "==":
                    return new Token(TokenType.Equal, op);
                case "!=":
                    return new Token(TokenType.NotEqual, op);
                case "&&":
                    return new Token(TokenType.And, op);
                case "||":
                    return new Token(TokenType.Or, op);
                case "&":
                    return new Token(TokenType.BitAnd, op);
                case "|":
                    return new Token(TokenType.BitOr, op);
                case "^":
                    return new Token(TokenType.BitXor, op);
                case "?":
                    return new Token(TokenType.Conditional, op);
                case ":":
                    return new Token(TokenType.Colon, op);
                default:
                    throw new ExpressionEvaluatorException(
                        String.Format("unknown operator : {0}", op));
            }
        }

        protected static IToken MakeNumberToken(string number)
        {
            return new Token(TokenType.Integer, number);
        }

        protected static IToken MakeIdentifierToken(string identifier)
        {
            return new Token(TokenType.String, identifier);
        }

        protected static IToken MakeSeparatorToken(string separator)
        {
            return new Token(TokenType.Separator, separator);
        }

        protected static IToken MakeInvalidToken(string invalid)
        {
            return new Token(TokenType.Invalid, invalid);
        }

        protected static IToken MakeTokenFromState(string token, ScannerState state)
        {
            switch (state)
            {
                case ScannerState.Identifier:
                    return MakeIdentifierToken(token);
                case ScannerState.Number:
                    return MakeNumberToken(token);
                case ScannerState.Operator:
                    return MakeOperatorToken(token);
                default:
                    throw new ExpressionEvaluatorException(
                        String.Format("unknown token '{0}'", token));
            }
        }
    }
}
