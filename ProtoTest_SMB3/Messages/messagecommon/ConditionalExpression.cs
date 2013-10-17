using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public class ConditionalExpression : BaseExpression
    {
        private IExpression firstExpression;
        public IExpression FirstExpression
        {
            get { return firstExpression; }
            set { firstExpression = value; }
        }
        private IExpression secondExpression;
        public IExpression SecondExpression
        {
            get { return secondExpression; }
            set { secondExpression = value; }
        }
        private IExpression thirdExpression;
        public IExpression ThirdExpression
        {
            get { return thirdExpression; }
            set { thirdExpression = value; }
        }

        public ConditionalExpression(IExpression firstExpression, IExpression secondExpression, IExpression thirdExpression)
        {
            this.firstExpression = firstExpression;
            this.secondExpression = secondExpression;
            this.thirdExpression = thirdExpression;
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
