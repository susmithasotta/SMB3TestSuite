using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public enum BinaryExpressionType
    {
        And,
        Or,
        Equal,
        NotEqual,
        LesserOrEqual,
        GreaterOrEqual,
        Lesser,
        Greater,
        Minus,
        Plus,
        Mod,
        Div,
        Multiply,
        ShiftLeft,
        ShiftRight,
        BitAnd,
        BitOr,
        BitXor,
    }
    public class BinaryExpression : BaseExpression
    {
        private BinaryExpressionType type;
        public BinaryExpressionType Type
        {
            get { return type; }
            set { type = value; }
        }

        private IExpression leftExpression;
        public IExpression LeftExpression
        {
            get { return leftExpression; }
            set { leftExpression = value; }
        }
        private IExpression rightExpression;
        public IExpression RightExpression
        {
            get { return rightExpression; }
            set { rightExpression = value; }
        }

        public BinaryExpression(BinaryExpressionType type, IExpression leftExpression, IExpression rightExpression)
        {
            this.type = type;
            this.leftExpression = leftExpression;
            this.rightExpression = rightExpression;
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
