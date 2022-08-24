using System;

namespace Qurl.Exceptions
{
    public class QurlException : Exception
    {
        public QurlException()
        {
        }

        public QurlException(string message) : base(message)
        {
        }

        public QurlException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
