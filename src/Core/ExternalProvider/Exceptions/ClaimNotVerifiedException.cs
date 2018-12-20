using System;
using System.Runtime.Serialization;

namespace Core.ExternalProvider.Exceptions
{
    public class ClaimNotVerifiedException : Exception
    {
        public ClaimNotVerifiedException()
        {
        }

        public ClaimNotVerifiedException(string message) : base(message)
        {
        }

        public ClaimNotVerifiedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ClaimNotVerifiedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
