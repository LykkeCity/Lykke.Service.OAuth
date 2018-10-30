using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class PasswordIsPwndException : Exception
    {
        public PasswordIsPwndException()
        {
        }

        public PasswordIsPwndException(string message) : base(message)
        {
        }

        public PasswordIsPwndException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PasswordIsPwndException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
