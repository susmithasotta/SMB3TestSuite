using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public enum UnaryExpressionType
    {
        Not,
        BitNot,
        Negative,
        Positive,
        Dereference,
    }
    public class UnaryExpression : BaseExpression
    {
        private UnaryExpressionType type;
        public UnaryExpressionType Type
        {
            get { return type; }
            set { type = value; }
        }
        private IExpression expression;
        public IExpression Expression
        {
            get { return expression; }
            set { expression = value; }
        }
        public UnaryExpression(UnaryExpressionType type, IExpression expression)
        {
            this.type = type;
            this.expression = expression;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException("visitor");
            }
            visitor.Visit(this);
        }
    }
}
