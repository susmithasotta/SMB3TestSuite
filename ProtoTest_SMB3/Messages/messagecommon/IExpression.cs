using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public interface IExpression
    {
        void Accept(IExpressionVisitor visitor);
    }
}
