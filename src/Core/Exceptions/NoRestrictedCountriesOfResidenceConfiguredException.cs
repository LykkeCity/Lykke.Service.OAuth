using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    /// <summary>
    ///     Exception for invalid configuration of restricted countries of residence.
    /// </summary>
    public class NoRestrictedCountriesOfResidenceConfiguredException : Exception
    {
        public NoRestrictedCountriesOfResidenceConfiguredException()
        {
        }

        public NoRestrictedCountriesOfResidenceConfiguredException(string message) : base(message)
        {
        }

        public NoRestrictedCountriesOfResidenceConfiguredException(string message, Exception innerException) : base(
            message, innerException)
        {
        }

        protected NoRestrictedCountriesOfResidenceConfiguredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
