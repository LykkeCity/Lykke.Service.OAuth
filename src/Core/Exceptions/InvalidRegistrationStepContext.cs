using System;
using System.Runtime.Serialization;
using Core.Registration;

namespace Core.Exceptions
{
    public class InvalidRegistrationStepContext : Exception
    {
        public RegistrationStep Step { get; }

        public InvalidRegistrationStepContext()
        {
        }

        public InvalidRegistrationStepContext(RegistrationStep step, string message = null) : base(message ?? "Invalid registration step context")
        {
            Step = step;
        }

        public InvalidRegistrationStepContext(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidRegistrationStepContext(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
