using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public class Token : IToken
    {
        private string text;
        private TokenType type;

        public Token(TokenType type)
        {
            this.type = type;
        }

        public Token(TokenType type, string text)
        {
            this.type = type;
            this.text = text;
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        public virtual TokenType Type
        {
            get
            {
                return type;
            }

            set
            {
                this.type = value;
            }
        }
    }
}
