using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public class MultipleExpressionEvaluator
    {
        private string expression;
        public string Expression
        {
            get { return expression; }
            set { expression = value; }
        }

        private IEvaluationContext context;
        public IEvaluationContext Context
        {
            get { return context; }
            set { context = value; }
        }

        public MultipleExpressionEvaluator()
        {
        }
        public MultipleExpressionEvaluator(IEvaluationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "evaluation context must be provided");
            }

            this.context = context;
        }
        public MultipleExpressionEvaluator(IEvaluationContext context, string expression)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "evaluation context must be provided");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression", "expression must be provided");
            }

            this.expression = expression;
            this.context = context;
        }

        public IList<object> Evaluate()
        {
            IList<object> result = new List<object>();
            string[] exprs = expression.Split(',');

            for (int i = 0; i < exprs.Length; i++)
            {
                exprs[i] = exprs[i].Trim();
                SingleExpressionEvaluator evaluator =
                    new SingleExpressionEvaluator(context, exprs[i]);
                object x = evaluator.Evaluate();
                result.Add(x);
            }

            return result;
        }
    }
}
