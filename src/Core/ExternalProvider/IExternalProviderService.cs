using System;
using System.Threading.Tasks;
using Core.ExternalProvider.Exceptions;

namespace Core.ExternalProvider
{
        /// <summary>
    ///     Service for operations with External identity providers.
    /// </summary>
    public interface IExternalProviderService
    {
        /// <summary>
        ///     Save lykke user id by random guid, during login process through ironclad.
        /// </summary>
        /// <param name="lykkeUserId">Lykke user id.</param>
        /// <param name="ttl">Time to life for temporary data.</param>
        /// <returns>Randomly generated guid, that is saved to cookie during ironclad login through Lykke OAuth.</returns>
        Task<string> SaveLykkeUserIdForExternalLoginAsync(string lykkeUserId, TimeSpan ttl);

        /// <summary>
        ///     Get previously saved lykke user id.
        /// </summary>
        /// <param name="guid">Previously generated guid.</param>
        /// <returns>Saved user id, if it still exists in database. Or empty string if user id not exist.</returns>
        Task<string> GetLykkeUserIdForExternalLoginAsync(string guid);

        /// <summary>
        ///     Get provider id by <paramref name="iss" /> issuer claim value.
        /// </summary>
        /// <param name="iss">Iss claim value.</param>
        /// <returns>Provider id.</returns>
        /// <exception cref="ExternalProviderNotFoundException">Thrown when provider is not found by <paramref name="iss" />.</exception>
        string GetProviderId(string iss);

        /// <summary>
        ///     Get provider configuration by <paramref name="providerId" /> - provider id.
        /// </summary>
        /// <param name="providerId">Provider id.</param>
        /// <returns>Provider configuration.</returns>
        /// <exception cref="ExternalProviderNotFoundException">Thrown when provider is not found by <paramref name="providerId" />.</exception>
        ExternalIdentityProvider GetProviderConfiguration(string providerId);
    }
}
