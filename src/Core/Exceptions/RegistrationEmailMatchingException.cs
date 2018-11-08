using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class RegistrationEmailMatchingException : Exception
    {
        public string Email { get; }

        public RegistrationEmailMatchingException()
        {
        }

        public RegistrationEmailMatchingException(string email, string message = null) : base(message ?? "The email doesn't match to the one was provided during registration")
        {
            Email = email;
        }

        public RegistrationEmailMatchingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RegistrationEmailMatchingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
