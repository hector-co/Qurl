using System;
using System.Runtime.Serialization;

namespace Qurl.Abstractions.Exceptions
{
    public class QurlParameterFormatException : Exception
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
