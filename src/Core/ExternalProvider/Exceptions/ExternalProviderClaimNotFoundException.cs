using System;
using System.Runtime.Serialization;

namespace Core.ExternalProvider.Exceptions
{
    public class ExternalProviderClaimNotFoundException : Exception
    {
        public string ClaimName { get; }
        public string ExternalProviderId { get; }
        public string ExternalUserId { get; }

        public ExternalProviderClaimNotFoundException(
            string claimName,
            string externalProviderId,
            string externalUserId) : base(GetMessage(claimName, externalProviderId, externalUserId))
        {
            ClaimName = claimName;
            ExternalProviderId = externalProviderId;
            ExternalUserId = externalUserId;
        }

        public ExternalProviderClaimNotFoundException()
        {
        }

        protected ExternalProviderClaimNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ExternalProviderClaimNotFoundException(string message) : base(message)
        {
        }

        public ExternalProviderClaimNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private static string GetMessage(
            string claimName,
            string externalProviderId,
            string externalUserId)
        {
            return
                $"Claim: {claimName} not found for external user: {externalUserId}, from external provider: {externalProviderId}";
        }
    }
}
