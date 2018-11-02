using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class NoPasswordValidatorsConfiguredException : Exception
    {
        public NoPasswordValidatorsConfiguredException()
        {
        }

        public NoPasswordValidatorsConfiguredException(string message) : base(message)
        {
        }

        public NoPasswordValidatorsConfiguredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoPasswordValidatorsConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
