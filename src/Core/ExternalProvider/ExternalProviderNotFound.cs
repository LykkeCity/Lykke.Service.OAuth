using System;
using System.Runtime.Serialization;

namespace Core.ExternalProvider
{
    public class ExternalProviderNotFound : Exception
    {
        public ExternalProviderNotFound()
        {
        }

        public ExternalProviderNotFound(string message) : base(message)
        {
        }

        public ExternalProviderNotFound(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExternalProviderNotFound(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
