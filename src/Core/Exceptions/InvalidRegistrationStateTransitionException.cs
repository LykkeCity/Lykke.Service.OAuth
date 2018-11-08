using System;
using System.Runtime.Serialization;
using Core.Registration;

namespace Core.Exceptions
{
    public class InvalidRegistrationStateTransitionException : Exception
    {
        public RegistrationStep CurrentStep { get; }

        public RegistrationStep DestinationStep { get; }

        public InvalidRegistrationStateTransitionException()
        {
        }

        public InvalidRegistrationStateTransitionException(RegistrationStep currentStep, RegistrationStep destStep,
            string message = null) : base(message ?? "Invalid registration state transition")
        {
            CurrentStep = currentStep;
            DestinationStep = destStep;
        }

        public InvalidRegistrationStateTransitionException(string message, Exception innerException) : base(message,
            innerException)
        {
        }

        protected InvalidRegistrationStateTransitionException(SerializationInfo info, StreamingContext context) : base(
            info, context)
        {
        }
    }
}
