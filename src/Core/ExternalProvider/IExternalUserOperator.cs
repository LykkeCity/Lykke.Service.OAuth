using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Extensions;
using Core.ExternalProvider.Exceptions;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     External user operations.
    /// </summary>
    public interface IExternalUserOperator
    {
        /// <summary>
        ///     Create new lykke user from ironclad user.
        ///     Or get existing user, if it has been already linked to external provider.
        /// </summary>
        /// <param name="ironcladUser">Ironclad user</param>
        /// <param name="originalClaims">Lykke user</param>
        /// <returns>
        ///     Created lykke user, or existing user, if it has been already linked to external provider.
        /// </returns>
        /// <exception cref="AutoprovisionException">
        ///     Thrown when user autoprovisioning failed, based on external provider data.
        /// </exception>
        Task<LykkeUser> ProvisionIfNotExistAsync(
            IroncladUser ironcladUser,
            IEnumerable<Claim> originalClaims);

        /// <summary>
        ///     Authenticate user.
        /// </summary>
        /// <param name="lykkeUser">Lykke user.</param>
        /// <returns>Lykke user authentication context.</returns>
        /// <exception cref="AuthenticationException">Thrown when user authentication fails.</exception>
        Task<LykkeUserAuthenticationContext> CreateLykkeSessionAsync(LykkeUser lykkeUser);

        /// <summary>
        ///     Temporary saves lykke user id.
        /// </summary>
        /// <remarks>
        ///     This is needed to securely get lykke user id, if it's not saved in ironclad.
        /// </remarks>
        /// <param name="lykkeUserId">Lykke user id.</param>
        /// <returns>Completed task if everything is ok.</returns>
        Task SaveTempLykkeUserIdAsync(string lykkeUserId);

        /// <summary>
        ///     Get temporary saved lykke user id.
        /// </summary>
        /// <returns>Lykke user id if exists, else returns null.</returns>
        Task<string> GetTempLykkeUserIdAsync();

        /// <summary>
        ///     Clear temporary saved lykke user id.
        /// </summary>
        Task ClearTempLykkeUserIdAsync();

        /// <summary>
        ///     Saves sign in page context.
        /// </summary>
        /// <param name="originalUrl"></param>
        Task SaveLykkeSignInContextAsync(string originalUrl);

        /// <summary>
        ///     Get saved lykke sign in context.
        /// </summary>
        /// <returns>Sign in page context.</returns>
        Task<string> GetLykkeSignInContextAsync();

        /// <summary>
        ///     Clears saved lykke sign in context.
        /// </summary>
        Task ClearLykkeSignInContextAsync();

        /// <summary>
        ///     Associates lykke user with ironclad user.
        ///     Does nothing if users already associated.
        /// </summary>
        /// <param name="lykkeUser">Lykke user.</param>
        /// <param name="ironcladUser">Ironclad user.</param>
        /// <returns>Completed task if association is successful.</returns>
        Task AssociateIroncladUserAsync(LykkeUser lykkeUser, IroncladUser ironcladUser);

        /// <summary>
        ///     Authenticates lykke user without creating session.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="partnerId">Partner id.</param>
        /// <returns>Lykke user id.</returns>
        Task<string> AuthenticateLykkeUserAsync(string username, string password, string partnerId);

        /// <summary>
        ///     Save ironclad request.
        /// </summary>
        /// <param name="redirectUrl">Url to redirect after ironclad login.</param>
        Task SaveIroncladRequestAsync(string redirectUrl);

        /// <summary>
        ///     Clear saved ironclad request.
        /// </summary>
        Task ClearIroncladRequestAsync();

        /// <summary>
        ///     Get saved ironclad request.
        /// </summary>
        /// <returns>Url if request exists, null otherwise.</returns>
        Task<string> GetIroncladRequestAsync();

        /// <summary>
        ///     End temporary user session.
        /// </summary>
        Task EndUserSessionAsync();
    }
}
