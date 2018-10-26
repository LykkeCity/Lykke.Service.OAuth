using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class BCryptWorkFactorInvalidException : Exception
    {
        public int WorkFactor { get; }

        public BCryptWorkFactorInvalidException()
        {
        }

        public BCryptWorkFactorInvalidException(string message) : base(message)
        {
        }

        public BCryptWorkFactorInvalidException(int workFactor) : base("Work factor is invalid")
        {
            WorkFactor = workFactor;
        }

        public BCryptWorkFactorInvalidException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BCryptWorkFactorInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
