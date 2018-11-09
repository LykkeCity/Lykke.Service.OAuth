using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class PasswordIsPwnedException : Exception
    {
        public PasswordIsPwnedException()
        {
        }

        public PasswordIsPwnedException(string message = null) : base(message ?? "Password has been previously exposed in data breaches.")
        {
        }

        public PasswordIsPwnedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PasswordIsPwnedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
