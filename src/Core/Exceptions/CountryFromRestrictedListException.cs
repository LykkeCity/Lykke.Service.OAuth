using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class CountryFromRestrictedListException : Exception
    {
        public string Country { get; }

        public CountryFromRestrictedListException()
        {
        }

        public CountryFromRestrictedListException(string country, string message = null) : base(message ?? "Country is from restricted countries list")
        {
            Country = country;
        }

        public CountryFromRestrictedListException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CountryFromRestrictedListException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
