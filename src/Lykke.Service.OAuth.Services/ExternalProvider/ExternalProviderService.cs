using System.Collections.Generic;
using Core.ExternalProvider;
using Core.ExternalProvider.Exceptions;
using JetBrains.Annotations;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    /// <inheritdoc/>
    [UsedImplicitly]
    public class ExternalProviderService : IExternalProviderService
    {
        private static readonly Dictionary<string, ExternalIdentityProvider> ExternalProviders =
            new Dictionary<string, ExternalIdentityProvider>();

        private static readonly Dictionary<string, string> IssToProviderId = new Dictionary<string, string>();

        public ExternalProviderService(IEnumerable<ExternalIdentityProvider> externalIdentityProviders)
        {
            foreach (var provider in externalIdentityProviders)
            {
                if (provider == null) continue;
                
                ExternalProviders.Add(provider.Id, provider);

                foreach (var iss in provider.ValidIssuers) 
                    IssToProviderId.Add(iss, provider.Id);
            }
        }

        /// <inheritdoc/>
        public string GetProviderId(string iss)
        {
            var notFoundException = new ExternalProviderNotFoundException($"Provider id not found by specified issuer: {iss}");

            if (string.IsNullOrWhiteSpace(iss))
                throw notFoundException;

            if (IssToProviderId.TryGetValue(iss, out var providerId)) return providerId;

            throw notFoundException;
        }

        /// <inheritdoc/>
        public ExternalIdentityProvider GetProviderConfiguration(string providerId)
        {
            var notFoundException = new ExternalProviderNotFoundException($"Provider not found by specified providerId: {providerId}");
            
            if (string.IsNullOrWhiteSpace(providerId))
                throw notFoundException;
            
            if (ExternalProviders.TryGetValue(providerId, out var provider)) return provider;

            throw notFoundException;
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetProviderClaimMapping(string providerId)
        {
            var providerConfig = GetProviderConfiguration(providerId);
            return providerConfig.ClaimsMapping;
        }
    }
}
