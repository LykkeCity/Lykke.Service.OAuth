using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class PasswordIsEmptyException : Exception
    {
        public PasswordIsEmptyException()
        {
        }

        public PasswordIsEmptyException(string message = null) : base(message ?? "Password is empty.")
        {
        }

        public PasswordIsEmptyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PasswordIsEmptyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
