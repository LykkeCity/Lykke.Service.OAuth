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
        ///     Associate lykke user with external user.
        /// </summary>
        /// <param name="provider">External provider name.</param>
        /// <param name="externalUserId">External user id.</param>
        /// <param name="lykkeUserId">Lykke user id.</param>
        /// <returns>Completed Task if user was associated.</returns>
        /// <exception cref="ExternalUserAlreadyAssociatedException">Thrown when external user already associated with lykke user.</exception>
        Task AssociateExternalUserAsync(string provider, string externalUserId, string lykkeUserId);

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
        /// <exception cref="ExternalProviderClaimNotFoundException">Thrown when required claim is missing.</exception>
        /// <exception cref="UserAutoprovisionFailedException">
        ///     Thrown when user autoprovisioning failed, based on external provider
        ///     data .
        /// </exception>
        Task<ClientAccountInformationModel> ProvisionIfNotExistAsync(ClaimsPrincipal principal);

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

        //TODO:@gafanasiev Add summary
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ironcladUserId"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task AddClaimToIroncladUser(string ironcladUserId, string type, object value);
    }
}
