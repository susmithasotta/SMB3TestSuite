using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public enum ValueExpressionType
    {
        Integer,
        Variable,
        Null,
    }
    public class ValueExpression : BaseExpression
    {
        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        private ValueExpressionType type;
        public ValueExpressionType Type
        {
            get { return type; }
            set { type = value; }
        }

        public ValueExpression(string text, ValueExpressionType type)
        {
            this.text = text;
            this.type = type;
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
