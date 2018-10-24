using System;
using System.Threading.Tasks;
using Core.ExternalProvider;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using Lykke.Service.ClientAccount.Client.Models;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    /// <inheritdoc/>
    public class ExternalUserService : IExternalUserService
    {
        private readonly IClientAccountClient _clientAccountClient;

        public ExternalUserService(
            IClientAccountClient clientAccountClient)
        {
            _clientAccountClient = clientAccountClient;
        }

        /// <inheritdoc/>
        public async Task<ClientAccountInformationModel> ProvisionIfNotExistAsync(ExternalClientProvisionModel model)
        {
            var lykkeAccount =
                await _clientAccountClient.GetClientByExternalIdentityProvider(
                    model.ExternalIdentityProviderId,
                    model.ExternalUserId) ??
                await _clientAccountClient.ProvisionAsync(model);

            return lykkeAccount;
        }
    }
}
