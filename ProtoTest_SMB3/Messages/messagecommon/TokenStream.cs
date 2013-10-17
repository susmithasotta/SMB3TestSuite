using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public class TokenStream : ITokenStream
    {
        private ExpressionLexer lexer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public TokenStream(ExpressionLexer lexer)
        {
            this.lexer = lexer;
        }

        public IToken NextToken()
        {
            return lexer.GetNextToken();
        }
    }
}
