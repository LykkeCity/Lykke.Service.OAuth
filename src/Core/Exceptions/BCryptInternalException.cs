using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class BCryptInternalException : Exception
    {
        public BCryptInternalException()
        {
        }

        public BCryptInternalException(string message) : base(message)
        {
        }

        public BCryptInternalException(Exception innerException) : base("BCrypt library internal exception", innerException)
        {
        }

        public BCryptInternalException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BCryptInternalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
