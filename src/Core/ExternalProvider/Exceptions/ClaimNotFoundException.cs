using System;
using System.Runtime.Serialization;

namespace Core.ExternalProvider.Exceptions
{
    public class ClaimNotFoundException : Exception
    {
        public ClaimNotFoundException()
        {
        }

        public ClaimNotFoundException(string message) : base(message)
        {
        }

        public ClaimNotFoundException(string message, Exception innerException) : base(message,
            innerException)
        {
        }

        protected ClaimNotFoundException(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}
