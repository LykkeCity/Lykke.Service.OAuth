using System.Security.Claims;
using System.Threading.Tasks;
using Core.ExternalProvider.Exceptions;
using Lykke.Service.ClientAccount.Client.Models;
using Microsoft.AspNetCore.Authentication;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     External user operations.
    /// </summary>
    public interface IExternalUserOperator
    {
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
        /// <param name="lykkeUser">Lykke user.</param>
        /// <returns>Lykke user authentication context.</returns>
        Task<LykkeUserAuthenticationContext> AuthenticateAsync(LykkeUser lykkeUser);

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
        /// <param name="authenticateResult">External authentication result.</param>
        /// <returns>Currently authenticated external user.</returns>
        Task<LykkeUser> GetCurrentUserAsync(AuthenticateResult authenticateResult);

        /// <summary>
        ///     Get redirect url after external login.
        /// </summary>
        /// <param name="authenticateResult">External authentication result.</param>
        /// <returns>Url to which user shoul be redirected.</returns>
        string GetRedirectUrl(AuthenticateResult authenticateResult);

        /// <summary>
        ///     Signin user through default scheme.
        /// </summary>
        /// <param name="context">Lykke user authentication context.</param>
        /// <returns>Completed task if everything is ok.</returns>
        Task SignInAsync(LykkeUserAuthenticationContext context);
    }
}
