using System;
using System.Runtime.Serialization;

namespace Core.ExternalProvider
{
    public class ExternalProviderNotFoundException : Exception
    {
        public ExternalProviderNotFoundException()
        {
        }

        public ExternalProviderNotFoundException(string message) : base(message)
        {
        }

        public ExternalProviderNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExternalProviderNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
