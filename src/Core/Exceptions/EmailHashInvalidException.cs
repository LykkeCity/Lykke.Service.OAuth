using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class EmailHashInvalidException : Exception
    {
        public string Email { get; }

        public EmailHashInvalidException()
        {
        }

        public EmailHashInvalidException(string message) : base(message)
        {
            
        }

        public EmailHashInvalidException(string email, string message = null) : base(message ?? "Hash is invalid")
        {
            Email = email;
        }

        public EmailHashInvalidException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EmailHashInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
