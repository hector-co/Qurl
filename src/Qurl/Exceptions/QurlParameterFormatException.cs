using System;
using System.Runtime.Serialization;

namespace Qurl.Exceptions
{
    public class QurlParameterFormatException : QurlException
    {
        public QurlParameterFormatException()
        {
        }

        public QurlParameterFormatException(string message) : base(message)
        {
        }

        public QurlParameterFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected QurlParameterFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
