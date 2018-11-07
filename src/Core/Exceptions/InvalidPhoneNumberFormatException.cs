using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class InvalidPhoneNumberFormatException : Exception
    {
        public string PhoneNumber { get; }

        public InvalidPhoneNumberFormatException()
        {
        }

        public InvalidPhoneNumberFormatException(string phoneNumber, string message = null) : base(message ?? "Invalid phone number format")
        {
            PhoneNumber = phoneNumber;
        }

        public InvalidPhoneNumberFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidPhoneNumberFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
