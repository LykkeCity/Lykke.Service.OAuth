using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class RedirectUrlInvalidException : Exception
    {
        public RedirectUrlInvalidException()
        {
        }

        public RedirectUrlInvalidException(string message = null) : base(message ?? "Password has been previously exposed in data breaches.")
        {
        }

        public RedirectUrlInvalidException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RedirectUrlInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}