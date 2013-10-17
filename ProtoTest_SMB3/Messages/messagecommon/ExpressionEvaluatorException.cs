using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    [Serializable]
    public class ExpressionEvaluatorException : Exception
    {
        public ExpressionEvaluatorException()
        {
        }

        public ExpressionEvaluatorException(string message)
            : base(message)
        {
        }

        public ExpressionEvaluatorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ExpressionEvaluatorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
