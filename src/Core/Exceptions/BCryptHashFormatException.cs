using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class BCryptHashFormatException : Exception
    {
        public string Hash { get; }

        public BCryptHashFormatException()
        {
        }

        public BCryptHashFormatException(string hash, string message = null) : base(message ?? "Hash has invalid format")
        {
            Hash = hash;
        }

        public BCryptHashFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BCryptHashFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
