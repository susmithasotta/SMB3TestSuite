using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public class SingleExpressionEvaluator
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

        public SingleExpressionEvaluator()
        {
        }
        public SingleExpressionEvaluator(IEvaluationContext context)
        {
            this.context = context;
        }
        public SingleExpressionEvaluator(IEvaluationContext context, string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            this.expression = expression;
            this.context = context;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public object Evaluate()
        {
            if (expression == null)
            {
                throw new ExpressionEvaluatorException("the expression should not be empty or null");
            }

            object result;
            try
            {
                ExpressionLexer lexer = new ExpressionLexer(expression);
                ExpressionParser parser = new ExpressionParser(
                    new TokenStream(lexer),
                    context);

                ExpressionNode tree = parser.Parse();
                ExpressionVisitor visitor = new ExpressionVisitor(context);
                IExpression expr = ExpressionBuilder.Build(tree);
                expr.Accept(visitor);

                result = visitor.EvaluationResult;
            }
            catch (Exception e) // reports all unexpected exceptions
            {
                context.ReportError(
                    String.Format(
                    "failed to evaluate expression '{0}' : '{1}'",
                    expression,
                    e.Message));
                result = expression;
            }

            return result;
        }
    }
}
