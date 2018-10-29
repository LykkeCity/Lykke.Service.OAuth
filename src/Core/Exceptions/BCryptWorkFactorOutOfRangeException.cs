using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class BCryptWorkFactorOutOfRangeException : Exception
    {
        public int WorkFactor { get; }

        public BCryptWorkFactorOutOfRangeException()
        {
        }

        public BCryptWorkFactorOutOfRangeException(string message) : base(message)
        {
        }

        public BCryptWorkFactorOutOfRangeException(int workFactor) : base("Work factor is out of range")
        {
            WorkFactor = workFactor;
        }

        public BCryptWorkFactorOutOfRangeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BCryptWorkFactorOutOfRangeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
