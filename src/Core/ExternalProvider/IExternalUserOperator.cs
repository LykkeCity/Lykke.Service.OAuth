using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.ExternalProvider.Exceptions;
using Lykke.Service.ClientAccount.Client.Models;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     External user operations.
    /// </summary>
    public interface IExternalUserOperator
    {
        /// <summary>
        /// Create association 
        /// </summary>
        /// <param name="ironcladUserId">Ironclad user id.</param>
        /// <param name="lykkeUserId">Lykke user id.</param>
        /// <returns>True is association is successful.</returns>
        Task<bool> AssociateIroncladUserAsync(string ironcladUserId, string lykkeUserId);

        /// <summary>
        ///     Get associated lykke user id.
        /// </summary>
        /// <param name="ironcladUserId">Ironclad user id.</param>
        /// <returns>
        ///     Lykke user id if it was already associated.
        ///     Empty string if user is not associated.
        /// </returns>
        Task<string> GetIroncladAssociatedLykkeUserIdAsync(string ironcladUserId);

        /// <summary>
        ///     Create new lykke user from external provider data.
        ///     Or get existing user, if it has been already linked to external provider.
        /// </summary>
        /// <param name="principal">Claims principal data from external provider.</param>
        /// <returns>
        ///     Account information of created user, or existing user, if it has been already linked to external provider.
        /// </returns>
        /// <exception cref="ExternalProviderPhoneNotVerifiedException">Thrown when phone is not verified by external provider.</exception>
        /// <exception cref="ClaimNotFoundException">Thrown when required claim is missing.</exception>
        /// <exception cref="AutoprovisionException">
        ///     Thrown when user autoprovisioning failed, based on external provider data.
        /// </exception>
        Task<ClientAccountInformationModel> ProvisionIfNotExistAsync(ClaimsPrincipal principal);

        /// <summary>
        ///     Authenticate ironclad user.
        /// </summary>
        /// <param name="principal">Ironclad user principal.</param>
        /// <returns>Lykke user authentication context.</returns>
        Task<LykkeUserAuthenticationContext> AuthenticateAsync(ClaimsPrincipal principal);

        /// <summary>
        ///     When user is authenticated.
        ///     Generates a random guid and saves binding guid -> lykkeUserId to Redis for 2 minutes.
        ///     Then saves it to cookie.
        /// </summary>
        /// <remarks>
        ///     This is needed to securely get lykke user id, if it's not saved in ironclad.
        /// </remarks>
        /// <param name="lykkeUserId">Lykke user id.</param>
        /// <returns>Completed task if everything is ok.</returns>
        Task SaveLykkeUserIdAfterIroncladlLoginAsync(string lykkeUserId);

        /// <summary>
        ///     Get current external user.
        /// </summary>
        /// <returns>Currently authenticated external user.</returns>
        Task<ClaimsPrincipal> GetCurrentUserAsync();

        /// <summary>
        ///     Signin user through default scheme.
        /// </summary>
        /// <param name="context">Lykke user authentication context.</param>
        /// <returns>Completed task if everything is ok.</returns>
        Task SignInAsync(LykkeUserAuthenticationContext context);
    }
}
