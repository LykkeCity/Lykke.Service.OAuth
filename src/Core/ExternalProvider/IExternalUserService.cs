using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.ExternalProvider.Exceptions;
using Lykke.Service.ClientAccount.Client.Models;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     External user operations.
    /// </summary>
    public interface IExternalUserService
    {
        /// <summary>
        ///     Get associated lykke user id.
        /// </summary>
        /// <param name="provider">External provider name.</param>
        /// <param name="externalUserId">External user id.</param>
        /// <returns>
        ///     Lykke user id if it was already associated.
        ///     Empty string if user is not associated.
        /// </returns>
        Task<string> GetAssociatedLykkeUserIdAsync(string provider, string externalUserId);

        /// <summary>
        ///     Create new lykke user from external provider data.
        ///     Or get existing user, if it has been already linked to external provider.
        /// </summary>
        /// <param name="principal">Claims principal data from external provider.</param>
        /// <returns>
        ///     Account information of created user, or existing user, if it has been already linked to external provider.
        ///     null if failed to create or retrieve user.
        /// </returns>
        /// <exception cref="ExternalProviderNotFoundException">Thrown when external provider could not be found.</exception>
        /// <exception cref="ExternalProviderPhoneNotVerifiedException">Thrown when phone is not verified by external provider.</exception>
        /// <exception cref="ClaimNotFoundException">Thrown when required claim is missing.</exception>
        /// <exception cref="AutoprovisionException">
        ///     Thrown when user autoprovisioning failed, based on external provider
        ///     data .
        /// </exception>

        /// <summary>
        ///     Save lykke user id by random guid, during login process through ironclad.
        /// </summary>
        /// <param name="lykkeUserId">Lykke user id.</param>
        /// <param name="ttl">Time to life for temporary data.</param>
        /// <returns>Randomly generated guid, that is saved to cookie during ironclad login through Lykke OAuth.</returns>
        Task<string> SaveLykkeUserIdForExternalLoginAsync(string lykkeUserId, TimeSpan ttl);

        //TODO:@gafanasiev add summary.
        Task<LykkeUserAuthenticationContext> HandleExternalUserLogin(ClaimsPrincipal principal);

        Task SaveLykkeUserIdAfterExternalLoginAsync(ClaimsPrincipal principal);
    }
}
