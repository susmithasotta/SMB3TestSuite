using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public class FunctionExpression : BaseExpression
    {
        private string functionName;
        public string FunctionName
        {
            get { return functionName; }
            set { functionName = value; }
        }
        private IExpression expression;
        public IExpression Expression
        {
            get { return expression; }
            set { expression = value; }
        }
        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        public FunctionExpression(string functionName, string text, IExpression expression)
        {
            this.functionName = functionName;
            this.expression = expression;
            this.text = text;
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
