using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Core.Extensions;
using Core.ExternalProvider;
using Core.ExternalProvider.Exceptions;
using JetBrains.Annotations;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    /// <inheritdoc />
    [UsedImplicitly]
    public class ExternalUserService : IExternalUserService
    {
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IPersonalDataService _personalDataService;
        private readonly IExternalProviderService _externalProviderService;

        public ExternalUserService(
            IClientAccountClient clientAccountClient,
            IPersonalDataService personalDataService,
            IExternalProviderService externalProviderService)
        {
            _clientAccountClient = clientAccountClient;
            _personalDataService = personalDataService;
            _externalProviderService = externalProviderService;
        }

        /// <inheritdoc />
        public async Task<ClientAccountInformationModel> ProvisionIfNotExistAsync(ClaimsPrincipal principal)
        {
            var externalUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(externalUserId))
                throw new ExternalProviderClaimNotFoundException("Claim for external user id not found.");

            var issuer = principal.FindFirst(OpenIdConnectConstants.Claims.Issuer)?.Value;
            
            if (string.IsNullOrWhiteSpace(issuer))
                throw new ExternalProviderClaimNotFoundException("Claim for issuer id not found.");

            var externalIdentityProviderId = _externalProviderService.GetProviderId(issuer);

            var existingAccount = await _clientAccountClient.GetClientByExternalIdentityProvider(
                externalIdentityProviderId,
                externalUserId);

            if (existingAccount != null)
                return existingAccount;

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
                throw new ExternalProviderClaimNotFoundException(ClaimTypes.Email, externalIdentityProviderId,
                    externalUserId);

            var isPhoneVerified = principal.FindFirst(OpenIdConnectConstantsExt.Claims.PhoneNumberVerified)?.Value;

            if (string.IsNullOrWhiteSpace(isPhoneVerified))
                throw new ExternalProviderClaimNotFoundException(OpenIdConnectConstantsExt.Claims.PhoneNumberVerified,
                    externalIdentityProviderId,
                    externalUserId);

            if (!Convert.ToBoolean(isPhoneVerified))
                throw new ExternalProviderPhoneNotVerifiedException("Phone is not verified on provider side!");

            var phone = principal.FindFirst(ClaimTypes.MobilePhone)?.Value;

            if (string.IsNullOrWhiteSpace(phone))
                throw new ExternalProviderClaimNotFoundException(ClaimTypes.MobilePhone, externalIdentityProviderId,
                    externalUserId);

            var newAccount =
                await _clientAccountClient.ProvisionAsync(new ExternalClientProvisionModel
                {
                    Email = email,
                    ExternalIdentityProviderId = externalIdentityProviderId,
                    ExternalUserId = externalUserId,
                    Phone = phone
                });

            if (newAccount == null)
                throw new UserAutoprovisionFailedException("User autoprovision failed!");

            var claims = principal.Claims.Select(claim => new IdentityProviderOriginalClaim
            {
                Name = claim.Type,
                Value = claim.Value
            });

            await _personalDataService.SaveClaimsAsync(new IdentityProviderClaimsModel
            {
                ExternalProviderId = externalIdentityProviderId,
                ExternalUserId = externalUserId,
                LykkeClientId = newAccount.Id,
                EmailClaim = newAccount.Email,
                PhoneClaim = newAccount.Phone,
                OriginalClaims = claims
            });

            return newAccount;
        }
    }
}
