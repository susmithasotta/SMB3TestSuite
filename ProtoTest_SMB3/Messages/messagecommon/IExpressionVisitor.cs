using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public interface IExpressionVisitor
    {
        void Visit(IExpression expression);
        void Visit(UnaryExpression expression);
        void Visit(BinaryExpression expression);
        void Visit(ConditionalExpression expression);
        void Visit(ValueExpression expression);
        void Visit(FunctionExpression expression);
    }
}
