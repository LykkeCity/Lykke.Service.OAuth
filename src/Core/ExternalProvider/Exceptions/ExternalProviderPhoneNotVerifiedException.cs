using System;
using System.Runtime.Serialization;

namespace Core.ExternalProvider.Exceptions
{
    public class ExternalProviderPhoneNotVerifiedException : Exception
    {
        public ExternalProviderPhoneNotVerifiedException()
        {
        }

        public ExternalProviderPhoneNotVerifiedException(string message) : base(message)
        {
        }

        public ExternalProviderPhoneNotVerifiedException(string message, Exception innerException) : base(message,
            innerException)
        {
        }

        protected ExternalProviderPhoneNotVerifiedException(SerializationInfo info, StreamingContext context) : base(
            info, context)
        {
        }
    }
}
