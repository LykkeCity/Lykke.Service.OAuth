using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class ExternalUserAlreadyAssociatedException : Exception
    {
        public ExternalUserAlreadyAssociatedException()
        {
        }

        public ExternalUserAlreadyAssociatedException(string message) : base(message)
        {
        }

        public ExternalUserAlreadyAssociatedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExternalUserAlreadyAssociatedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
