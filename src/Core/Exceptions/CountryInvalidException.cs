using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class CountryInvalidException : Exception
    {
        public string Country { get; }

        public CountryInvalidException()
        {
        }

        public CountryInvalidException(string country, string message = null) : base(message ?? "Country is invalid.")
        {
            Country = country;
        }

        public CountryInvalidException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CountryInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
