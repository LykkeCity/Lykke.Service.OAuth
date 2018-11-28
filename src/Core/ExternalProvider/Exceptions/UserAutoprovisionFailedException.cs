using System;
using System.Runtime.Serialization;

namespace Core.ExternalProvider.Exceptions
{
    public class UserAutoprovisionFailedException : Exception
    {
        public UserAutoprovisionFailedException()
        {
        }

        public UserAutoprovisionFailedException(string message) : base(message)
        {
        }

        public UserAutoprovisionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UserAutoprovisionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
