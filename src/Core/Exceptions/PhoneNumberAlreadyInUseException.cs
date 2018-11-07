using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class PhoneNumberAlreadyInUseException : Exception
    {
        public string PhoneNumber { get; }

        public PhoneNumberAlreadyInUseException()
        {
        }

        public PhoneNumberAlreadyInUseException(string phoneNumber, string message = null) : base(message ?? "Phone number already in use")
        {
            PhoneNumber = phoneNumber;
        }

        public PhoneNumberAlreadyInUseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PhoneNumberAlreadyInUseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
