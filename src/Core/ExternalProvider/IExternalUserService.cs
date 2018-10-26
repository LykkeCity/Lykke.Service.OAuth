using System.Security.Claims;
using System.Threading.Tasks;
using Core.ExternalProvider.Exceptions;
using Lykke.Service.ClientAccount.Client.Models;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     External user operations.
    /// </summary>
    /// <exception cref="ExternalProviderNotFoundException">Thrown when external provider could not be found.</exception>
    /// <exception cref="ExternalProviderPhoneNotVerifiedException">Thrown when phone is not verified by external provider.</exception>
    /// <exception cref="ExternalProviderClaimNotFoundException">Thrown when required claim is missing.</exception>
    /// <exception cref="UserAutoprovisionFailedException">Thrown when user autoprovisioning failed, based on external provider data .</exception>
    public interface IExternalUserService
    {
        /// <summary>
        ///     Create new lykke user from external provider data.
        ///     Or get existing user, if it has been already linked to external provider.
        /// </summary>
        /// <param name="principal">Claims principal data from external provider.</param>
        /// <returns>
        ///     Account information of created user, or existing user, if it has been already linked to external provider.
        ///     null if failed to create or retrieve user.
        /// </returns>
        Task<ClientAccountInformationModel> ProvisionIfNotExistAsync(ClaimsPrincipal principal);
    }
}
