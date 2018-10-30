using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class RegistrationKeyNotFoundException : Exception
    {
        public RegistrationKeyNotFoundException()
        {
        }
        public RegistrationKeyNotFoundException(string message) : base(message)
        {
        }
        public RegistrationKeyNotFoundException(Exception innerException) : base("Redis library internal exception", innerException)
        {
        }
        public RegistrationKeyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
        protected RegistrationKeyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
