using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public class ExpressionNode : BaseNode
    {
        private IToken token;

        public ExpressionNode()
        {
        }

        public ExpressionNode(IToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            this.token = token;
        }

        public override TokenType Type
        {
            get
            {
                if (token == null)
                {
                    return TokenType.Invalid;
                }

                return token.Type;
            }
        }

        public override string Text
        {
            get
            {
                if (token == null)
                {
                    return null;
                }
                return token.Text;
            }
        }

        public override string ToString()
        {
            if (token == null)
            {
                return null;
            }

            return token.Text;
        }
    }
}
