using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class ClientNotFoundException : Exception
    {
        public string ClientId { get; }

        public ClientNotFoundException()
        {
        }

        public ClientNotFoundException(string clientId, string message = null) : base(message ?? "Client id is not found")
        {
            ClientId = clientId;
        }

        public ClientNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ClientNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
