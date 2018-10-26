using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class BCryptWorkFactorInconsistencyException : Exception
    {
        public BCryptWorkFactorInconsistencyException()
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

        public int WorkFactor { get; set; }
    }
}
