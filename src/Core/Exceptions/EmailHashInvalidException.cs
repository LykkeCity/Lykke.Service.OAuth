using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class EmailHashInvalidException : Exception
    {
        public EmailHashInvalidException()
        {
        }

        public EmailHashInvalidException(string email) : base("Hash is invalid")
        {
            Email = email;
        }

        public EmailHashInvalidException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EmailHashInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Email { get; set; }
    }
}
