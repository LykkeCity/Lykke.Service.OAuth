using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class BCryptWorkFactorInconsistencyException : Exception
    {
        public int WorkFactor { get; }

        public BCryptWorkFactorInconsistencyException()
        {
        }

        public BCryptWorkFactorInconsistencyException(string message) : base(message)
        {
        }

        public BCryptWorkFactorInconsistencyException(int workFactor) : base("BCrypt work factor value is invalid")
        {
            WorkFactor = workFactor;
        }

        public BCryptWorkFactorInconsistencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BCryptWorkFactorInconsistencyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
