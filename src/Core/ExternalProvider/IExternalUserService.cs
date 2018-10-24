using System.Threading.Tasks;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using Lykke.Service.ClientAccount.Client.Models;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     External user operations.
    /// </summary>
    public interface IExternalUserService
    {
        /// <summary>
        ///     Create new lykke user from external provider data.
        ///     Or get existing user, if it has been already linked to external provider.
        /// </summary>
        /// <param name="model">Data from external provider.</param>
        /// <returns>
        ///     Account information of created user, or existing user, if it has been already linked to external provider.
        ///     null if failed to create or retrieve user.
        /// </returns>
        Task<ClientAccountInformationModel> ProvisionIfNotExistAsync(ExternalClientProvisionModel model);
    }
}
