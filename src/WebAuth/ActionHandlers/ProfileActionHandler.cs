using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Core;
using Core.Extensions;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Abstractions.Services.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using WebAuth.Managers;

namespace WebAuth.ActionHandlers
{
    public class ProfileActionHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserManager _userManager;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IKycProfileService _kycProfileService;
        private readonly IPersonalDataService _personalDataService;

        public ProfileActionHandler(
            IHttpContextAccessor httpContextAccessor,
            IUserManager userManager,
            IClientAccountClient clientAccountClient,
            IKycProfileService kycProfileService
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _clientAccountClient = clientAccountClient;
            _kycProfileService = kycProfileService;
        }

        public async Task UpdatePersonalInformation(string clientId, string firstName, string lastName)
        {
            string fullname = $"{firstName} {lastName}";

            var changes = new KycPersonalDataChanges
            {
                Changer = RecordChanger.Client,
                Items = new Dictionary<string, JToken>
                {
                    {nameof(IPersonalData.FirstName), firstName},
                    {nameof(IPersonalData.LastName), lastName},
                    {nameof(IPersonalData.FullName), fullname}
                }
            };

            await _kycProfileService.UpdatePersonalDataAsync(clientId, changes);

            var clientAccount = await _clientAccountClient.GetByIdAsync(clientId);
            var clientEmail = await _personalDataService.GetEmailAsync(clientId);

            await _httpContextAccessor.HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme);

            var identity = await _userManager.CreateUserIdentityAsync(clientAccount.Id, clientEmail, clientEmail, true);

            await _httpContextAccessor.HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties());
        }
    }
}
