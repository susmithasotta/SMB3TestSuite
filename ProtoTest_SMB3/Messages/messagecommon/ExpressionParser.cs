using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public class ExpressionParser
    {
        private IEvaluationContext context;
        private ITokenStream stream;
        private IToken currentToken;
        private bool isInSizeOf;

        public ExpressionParser(ITokenStream stream, IEvaluationContext context)
        {
            this.stream = stream;
            this.context = context;
            GetNextToken();
        }

        public ExpressionNode Parse()
        {
            if (currentToken.Type == TokenType.EndOfFile)
                return new ExpressionNode(currentToken);

            ExpressionNode root = Conditional();

            if (currentToken.Type != TokenType.EndOfFile)
            {
                throw new ExpressionEvaluatorException(
                    String.Format("unexpected token '{0}'", currentToken.Text));
            }

            return root;
        }

        protected void GetNextToken()
        {
            currentToken = stream.NextToken();
        }

        protected void Expect(string expectedToken)
        {
            if (currentToken.Text == expectedToken)
                GetNextToken();
            else
            {
                throw new ExpressionEvaluatorException(
                    String.Format("expected '{0}'", expectedToken));
            }
        }

        protected bool Try(string token)
        {
            if (currentToken.Text == token)
            {
                GetNextToken();
                return true;
            }
            else
                return false;
        }

        #region Operations

        ExpressionNode Conditional()
        {
            ExpressionNode c = LazyOr();
            ExpressionNode root = c;
            if (Try("?"))
            {
                root = new ExpressionNode(new Token(TokenType.Conditional, "?"));
                ExpressionNode t = Conditional();
                Expect(":");
                ExpressionNode e = Conditional();
                root.AddChild(c);
                root.AddChild(t);
                root.AddChild(e);
            }

            return root;
        }

        protected ExpressionNode LazyOr()
        {
            ExpressionNode x = LazyAnd();
            ExpressionNode root = x;
            while (Try("||"))
            {
                ExpressionNode y = LazyAnd();
                root = new ExpressionNode(new Token(TokenType.Or, "||"));
                root.AddChild(x);
                root.AddChild(y);
            }
            return root;
        }

        protected ExpressionNode LazyAnd()
        {
            ExpressionNode x = BitOr();
            ExpressionNode root = x;
            while (Try("&&"))
            {
                ExpressionNode y = BitOr();
                root = new ExpressionNode(new Token(TokenType.And, "&&"));
                root.AddChild(x);
                root.AddChild(y);
            }
            return root;
        }

        protected ExpressionNode BitOr()
        {
            ExpressionNode x = BitXor();
            ExpressionNode root = x;
            while (Try("|"))
            {
                ExpressionNode y = BitXor();
                root = new ExpressionNode(new Token(TokenType.BitOr, "|"));
                root.AddChild(x);
                root.AddChild(y);
            }
            return root;
        }

        protected ExpressionNode BitXor()
        {
            ExpressionNode x = BitAnd();
            ExpressionNode root = x;
            while (Try("^"))
            {
                ExpressionNode y = BitAnd();
                root = new ExpressionNode(new Token(TokenType.BitXor, "^"));
                root.AddChild(x);
                root.AddChild(y);
            }
            return root;
        }

        protected ExpressionNode BitAnd()
        {
            ExpressionNode x = Relation();
            ExpressionNode root = x;
            while (Try("&"))
            {
                ExpressionNode y = Relation();
                root = new ExpressionNode(new Token(TokenType.BitAnd, "&"));
                root.AddChild(x);
                root.AddChild(y);
            }
            return root;
        }

        protected ExpressionNode Relation()
        {
            ExpressionNode x = Shift();
            ExpressionNode root = x;
            while (true)
            {
                if (Try("=="))
                {
                    ExpressionNode y = Shift();
                    x = root;
                    root = new ExpressionNode(new Token(TokenType.Equal, "=="));
                    root.AddChild(x);
                    root.AddChild(y);
                }
                else if (Try("!="))
                {
                    ExpressionNode y = Shift();
                    x = root;
                    root = new ExpressionNode(new Token(TokenType.NotEqual, "!="));
                    root.AddChild(x);
                    root.AddChild(y);
                }
                else if (Try(">="))
                {
                    ExpressionNode y = Shift();
                    x = root;
                    root = new ExpressionNode(new Token(TokenType.GreaterOrEqual, ">="));
                    root.AddChild(x);
                    root.AddChild(y);
                }
                else if (Try("<="))
                {
                    ExpressionNode y = Shift();
                    x = root;
                    root = new ExpressionNode(new Token(TokenType.LesserOrEqual, "<="));
                    root.AddChild(x);
                    root.AddChild(y);
                }
                else if (Try(">"))
                {
                    ExpressionNode y = Shift();
                    x = root;
                    root = new ExpressionNode(new Token(TokenType.Greater, ">"));
                    root.AddChild(x);
                    root.AddChild(y);
                }
                else if (Try("<"))
                {
                    ExpressionNode y = Shift();
                    x = root;
                    root = new ExpressionNode(new Token(TokenType.Lesser, "<"));
                    root.AddChild(x);
                    root.AddChild(y);
                }
                else
                    break;
            }
            return root;
        }

        protected ExpressionNode Shift()
        {
            ExpressionNode x = Add();
            ExpressionNode root = x;
            while (true)
            {
                if (Try("<<"))
                {
                    ExpressionNode y = Add();
                    x = root;
                    root = new ExpressionNode(new Token(TokenType.ShiftLeft, "<<"));
                    root.AddChild(x);
                    root.AddChild(y);
                }
                else if (Try(">>"))
                {
                    ExpressionNode y = Add();
                    x = root;
                    root = new ExpressionNode(new Token(TokenType.ShiftRight, ">>"));
                    root.AddChild(x);
                    root.AddChild(y);
                }
                else
                    break;
            }
            return root;
        }

        protected ExpressionNode Add()
        {
            ExpressionNode x = Multiply();
            ExpressionNode root = x;
            while (true)
            {
                ExpressionNode oldRoot = root;
                if (Try("+"))
                {
                    ExpressionNode y = Multiply();
                    root = new ExpressionNode(new Token(TokenType.Plus, "+"));
                    root.AddChild(oldRoot);
                    root.AddChild(y);
                }
                else if (Try("-"))
                {
                    ExpressionNode y = Multiply();
                    root = new ExpressionNode(new Token(TokenType.Minus, "-"));
                    root.AddChild(oldRoot);
                    root.AddChild(y);
                }
                else
                    break;
            }
            return root;
        }

        protected ExpressionNode Multiply()
        {
            ExpressionNode x = Unary();
            ExpressionNode root = x;
            while (true)
            {
                ExpressionNode oldRoot = root;
                if (Try("*"))
                {
                    ExpressionNode y = Unary();
                    root = new ExpressionNode(new Token(TokenType.Multiply, "*"));
                    root.AddChild(oldRoot);
                    root.AddChild(y);
                }
                else if (Try("/"))
                {
                    ExpressionNode y = Unary();
                    root = new ExpressionNode(new Token(TokenType.Divide, "/"));
                    root.AddChild(oldRoot);
                    root.AddChild(y);
                }
                else if (Try("%"))
                {
                    ExpressionNode y = Unary();
                    root = new ExpressionNode(new Token(TokenType.Mod, "%"));
                    root.AddChild(oldRoot);
                    root.AddChild(y);
                }
                else
                    break;
            }
            return root;
        }

        protected ExpressionNode Unary()
        {
            ExpressionNode root;
            ExpressionNode x;
            if (Try("!"))
            {
                root = new ExpressionNode(new Token(TokenType.Not, "!"));
                x = Unary();
                root.AddChild(x);
            }
            else if (Try("~"))
            {
                root = new ExpressionNode(new Token(TokenType.BitNot, "~"));
                x = Unary();
                root.AddChild(x);
            }
            else if (Try("-"))
            {
                root = new ExpressionNode(new Token(TokenType.Negative, "-"));
                x = Unary();
                root.AddChild(x);
            }
            else if (Try("+"))
            {
                root = new ExpressionNode(new Token(TokenType.Positive, "+"));
                x = Unary();
                root.AddChild(x);
            }
            else if (Try("*"))
            {
                root = new ExpressionNode(new Token(TokenType.Dereference, "*"));
                x = Unary();
                root.AddChild(x);
            }
            else
                root = Primary();
            return root;
        }

        protected ExpressionNode Sizeof()
        {
            Token sizeofToken = new Token(TokenType.SizeOf, "sizeof");
            if (Try("sizeof"))
            {
                isInSizeOf = true;
                ExpressionNode root = new ExpressionNode(sizeofToken);
                ExpressionNode child = Primary();
                root.AddChild(child);
                isInSizeOf = false;
                return root;
            }
            else
                return Literal();
        }

        protected ExpressionNode Literal()
        {

            if (currentToken.Text.Length > 0)
                if (currentToken.Type == TokenType.Integer)
                {
                    int res;
                    if (TryConvertNumber(currentToken.Text, out res))
                    {
                        ExpressionNode integer = new ExpressionNode(
                            new Token(TokenType.Integer, res.ToString()));
                        GetNextToken();
                        return integer;
                    }
                    else
                    {
                        throw new ExpressionEvaluatorException(
                            String.Format("invalid number '{0}'", currentToken.Text));
                    }
                }
                else if (currentToken.Type == TokenType.String)
                {
                    int x;

                    if (!context.TryResolveSymbol(currentToken.Text, out x))
                    {
                        string tokenText = currentToken.Text;
                        while (true)
                        {
                            if (context.Variables != null
                                && !context.Variables.ContainsKey(currentToken.Text)
                                && !DatatypeInfoProvider.IsPredefinedDatatype(currentToken.Text)
                                && !DatatypeInfoProvider.isPredefinedModifier(currentToken.Text))
                            {
                                string typeInfo;
                                if (context.TryResolveCustomType(currentToken.Text, out typeInfo))
                                {
                                    tokenText = typeInfo;
                                    break;
                                }
                                else
                                {
                                    context.ReportError(
                                        String.Format("cannot resolve symbol '{0}'", currentToken.Text));
                                    break;
                                }
                            }
                            if (context.Variables != null
                                && context.Variables.ContainsKey(currentToken.Text))
                            {
                                tokenText = currentToken.Text;
                                break;
                            }

                            // treat predefinedModifier as empty node
                            if (DatatypeInfoProvider.isPredefinedModifier(currentToken.Text))
                            {
                                GetNextToken();
                            }
                            else if (DatatypeInfoProvider.IsPredefinedDatatype(currentToken.Text))
                            {
                                tokenText = currentToken.Text;
                                break;
                            }
                        }

                        // in order to get pointer type size and avoid related issue
                        bool isPointerType = false;
                        GetNextToken();
                        if (isInSizeOf)
                        {
                            while (currentToken.Text == "*")
                            {
                                GetNextToken();
                                isPointerType = true;
                            } 
                        }

                        if (isPointerType)
                        {
                            ExpressionNode integer = new ExpressionNode(
                                new Token(TokenType.Integer, "4"));
                            return integer;
                        }
                        ExpressionNode variable = new ExpressionNode(
                            new Token(TokenType.Variable, tokenText));

                        return variable;
                    }
                    else if (isInSizeOf)
                    {
                        string tokenText = currentToken.Text;

                        GetNextToken();

                        //ignoring other expression
                        while (currentToken.Text != ")")
                        {
                            GetNextToken();
                        }

                        return new ExpressionNode(
                            new Token(TokenType.Variable, tokenText));
                    }
                    else
                    {
                        GetNextToken();
                        return new ExpressionNode(
                            new Token(TokenType.Integer, x.ToString()));
                    }
                }
                else
                    throw new ExpressionEvaluatorException(
                        String.Format("unexpected token '{0}'", currentToken));
            else
                throw new ExpressionEvaluatorException("unexpected end of input");
        }

        protected ExpressionNode Primary()
        {
            if (Try("("))
            {
                ExpressionNode x = Conditional();
                Expect(")");
                return x;
            }
            else
                return Sizeof();
        }

        public static bool TryConvertNumber(string representation, out int value)
        {
            if (representation == null)
            {
                throw new ArgumentNullException("representation");
            }

            value = 1; // to prevent div zero errors
            try
            {
                representation = representation.ToLower();
                int b = GetNumberBase(ref representation);
                value = (int)Convert.ToInt64(representation, b);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool TryConvertType(string representation, out Type type)
        {
            type = null;
            if (representation == null)
            {
                throw new ArgumentNullException("representation");
            }

            representation = representation.ToLower();
            int b = GetNumberBase(ref representation);
            long value = long.MinValue;
            try
            {
                value = Convert.ToInt64(representation, b);
                if ((ulong)value <= (ulong)long.MaxValue)
                {
                    type = typeof(long);
                }
                else
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (OverflowException) 
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }

            try
            {
                if (value == Convert.ToUInt32(representation, b))
                {
                    type = typeof(uint);
                }
            }
            catch (ArgumentException) { }
            catch (OverflowException) { }
            catch (FormatException) { }

            try
            {
                if (value == Convert.ToInt32(representation, b))
                {
                    type = typeof(int);
                }
            }
            catch (ArgumentException) { }
            catch (OverflowException) { }
            catch (FormatException) { }

            try
            {
                if (value == Convert.ToInt16(representation, b))
                {
                    type = typeof(short);
                }
            }
            catch (ArgumentException) { }
            catch (OverflowException) { }
            catch (FormatException) { }

            try
            {
                if (value == Convert.ToUInt16(representation, b))
                {
                    type = typeof(ushort);
                }
            }
            catch (ArgumentException) { }
            catch (OverflowException) { }
            catch (FormatException) { }

            try
            {
                if (value == Convert.ToByte(representation, b))
                {
                    type = typeof(byte);
                }
            }
            catch (ArgumentException) { }
            catch (OverflowException) { }
            catch (FormatException) { }

            return true;
        }

        static int GetNumberBase(ref string representation)
        {
            int b = 10;
            representation = representation.ToLower();
            if (representation.StartsWith("0x"))
            {
                representation = representation.Substring(2);
                b = 16;
            }
            else if (representation.StartsWith("0b"))
            {
                representation = representation.Substring(2);
                b = 2;
            }
            else if (representation.StartsWith("0"))
                b = 8;

            return b;
        }
        #endregion
    }
}
