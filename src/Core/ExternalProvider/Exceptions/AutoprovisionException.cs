using System;
using System.Runtime.Serialization;

namespace Core.ExternalProvider.Exceptions
{
    public class AutoprovisionException : Exception
    {
        public AutoprovisionException()
        {
        }

        public AutoprovisionException(string message) : base(message)
        {
        }

        public AutoprovisionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AutoprovisionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
