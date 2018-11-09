using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class PasswordIsNotComplexException : Exception
    {
        public PasswordIsNotComplexException()
        {
        }

        public PasswordIsNotComplexException(string message = null) : base(message ?? "Password is not complex.")
        {
        }

        public PasswordIsNotComplexException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PasswordIsNotComplexException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
