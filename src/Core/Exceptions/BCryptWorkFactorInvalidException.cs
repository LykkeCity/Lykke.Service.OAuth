using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class BCryptWorkFactorInvalidException : Exception
    {
        public BCryptWorkFactorInvalidException()
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

        public int WorkFactor { get; set; }
    }
}
