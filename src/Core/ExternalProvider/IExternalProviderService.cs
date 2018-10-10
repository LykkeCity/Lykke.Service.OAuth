using System.Collections.Generic;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     Service for operations with External identity providers.
    /// </summary>
    public interface IExternalProviderService
    {
        /// <summary>
        ///     Get provider id by <paramref name="iss" /> issuer claim value.
        /// </summary>
        /// <param name="iss">Iss claim value.</param>
        /// <returns>Provider id.</returns>
        /// <exception cref="ExternalProviderNotFound">Thrown when provider is not found by <paramref name="iss" />.</exception>
        string GetProviderId(string iss);

        /// <summary>
        ///     Get provider configuration by <paramref name="providerId" /> - provider id.
        /// </summary>
        /// <param name="providerId">Provider id.</param>
        /// <returns>Provider configuration.</returns>
        /// <exception cref="ExternalProviderNotFound">Thrown when provider is not found by <paramref name="providerId" />.</exception>
        ExternalIdentityProvider GetProviderConfiguration(string providerId);

        /// <summary>
        ///     Get claim mapping dictionary by <paramref name="providerId" /> - provider id.
        /// </summary>
        /// <param name="providerId">Provider id.</param>
        /// <returns>Provider claim mapping dictionary.</returns>
        /// <exception cref="ExternalProviderNotFound">Thrown when provider is not found by <paramref name="providerId" />.</exception>
        IDictionary<string, string> GetProviderClaimMapping(string providerId);
    }
}
