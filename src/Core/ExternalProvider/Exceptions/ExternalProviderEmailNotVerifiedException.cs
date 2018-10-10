using System;
using System.Runtime.Serialization;

namespace Core.ExternalProvider.Exceptions
{
    public class ExternalProviderEmailNotVerifiedException : Exception
    {
        public ExternalProviderEmailNotVerifiedException()
        {
        }

        public ExternalProviderEmailNotVerifiedException(string message) : base(message)
        {
        }

        public ExternalProviderEmailNotVerifiedException(string message, Exception innerException) : base(message,
            innerException)
        {
        }

        protected ExternalProviderEmailNotVerifiedException(SerializationInfo info, StreamingContext context) : base(
            info, context)
        {
        }
    }
}
