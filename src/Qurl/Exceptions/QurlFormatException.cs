using System;

namespace Qurl.Exceptions
{
    public class QurlFormatException : QurlException
    {
        public QurlFormatException()
        {
        }

        public QurlFormatException(string message) : base(message)
        {
        }

        public QurlFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
