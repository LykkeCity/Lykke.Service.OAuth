using System;
using System.Net;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Common;
using Common.Log;
using Core.Application;
using Core.Bitcoin;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.Session;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Extensions;
using WebAuth.Models;

namespace WebAuth.Controllers
{
    public class UserinfoController : Controller
    {
        private readonly ILog _log;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IKycProfileServiceV2 _kycProfileService;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IPersonalDataService _personalDataService;

        public UserinfoController(
            ILog log,
            IApplicationRepository applicationRepository,
            IKycProfileServiceV2 kycProfileService,
            IClientSessionsClient clientSessionsClient,
            IWalletCredentialsRepository walletCredentialsRepository,
            IClientAccountClient clientAccountClient,
            IPersonalDataService personalDataService)
        {
            _log = log;
            _applicationRepository = applicationRepository;
            _kycProfileService = kycProfileService;
            _clientSessionsClient = clientSessionsClient;
            _walletCredentialsRepository = walletCredentialsRepository;
            _clientAccountClient = clientAccountClient;
            _personalDataService = personalDataService;
        }

        [HttpGet("~/connect/userinfo")]
        [Authorize(AuthenticationSchemes = OAuthValidationConstants.Schemes.Bearer)]
        public IActionResult GetUserInfo()
        {
            var userInfo = new UserInfoViewModel
            {
                Email = User.GetClaim(OpenIdConnectConstants.Claims.Email),
                FirstName = User.GetClaim(OpenIdConnectConstants.Claims.GivenName),
                LastName = User.GetClaim(OpenIdConnectConstants.Claims.FamilyName)
            };
            return Json(userInfo);
        }

        [HttpGet("~/getkycstatus")]
        public async Task<IActionResult> GetKycStatus(string email)
        {
            if (!email.IsValidEmailAndRowKey())
                return BadRequest("Invalid email");

            var applicationId = HttpContext.GetApplicationId();

            if (!applicationId.IsValidPartitionOrRowKey())
                return BadRequest("Invalid applicationId");

            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null)
                return BadRequest("Application Id Incorrect!");

            ClientAccountInformationModel client = await GetClientByEmailAsync(email);

            if (client == null)
                return NotFound("Client not found!");

            try
            {
                var kycStatus = await _kycProfileService.GetStatusAsync(client.Id, KycProfile.Default);
                return Json(kycStatus.Name);
            }
            catch(Exception)
            {
                return StatusCode((int) HttpStatusCode.InternalServerError, "Can't get KYC status");
            }
        }

        [HttpGet("~/getidbyemail")]
        public async Task<IActionResult> GetIdByEmail(string email)
        {
            if (!email.IsValidEmailAndRowKey())
                return BadRequest("Invalid email");

            var applicationId = HttpContext.GetApplicationId();

            if (!applicationId.IsValidPartitionOrRowKey())
                return BadRequest("Invalid applicationId");

            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null)
                return BadRequest("Application Id Incorrect!");

            ClientAccountInformationModel client = await GetClientByEmailAsync(email);

            if (client == null)
                return NotFound("Client not found!");

            return Json(client.Id);
        }

        [HttpGet("~/getemailbyid")]
        public async Task<IActionResult> GetEmailById(string id)
        {
            if (!id.IsValidPartitionOrRowKey() && !id.IsGuid())
                return BadRequest("Invalid id");

            var applicationId = HttpContext.GetApplicationId();

            if (!applicationId.IsValidPartitionOrRowKey())
                return BadRequest("Invalid applicationId");

            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null)
                return BadRequest("Application Id Incorrect!");

            ClientModel client = await GetClientByIdAsync(id);

            if (client == null)
                return NotFound("Client not found!");

            var clientEmail = await _personalDataService.GetEmailAsync(client.Id);

            return Json(clientEmail);
        }

        [HttpGet("~/getlykkewallettoken")]
        [Authorize(AuthenticationSchemes = OAuthValidationConstants.Schemes.Bearer)]
        public async Task<IActionResult> GetLykkewalletToken()
        {
            var applicationId = HttpContext.GetApplicationId();

            if (!applicationId.IsValidPartitionOrRowKey())
                return BadRequest("Invalid applicationId");

            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null)
                return BadRequest("Application Id Incorrect!");

            var clientId = User.Identity.GetClientId();

            if (clientId == null)
                return NotFound("Can't get clientId from claims");

            ClientModel clientAccount = await GetClientByIdAsync(clientId);

            if (clientAccount == null)
                return NotFound("Client not found");

            try
            {
                var authResult = await _clientSessionsClient.Authenticate(clientAccount.Id, "oauth server", application: app.Type);
                return Json(new { Token = authResult.SessionToken, authResult.AuthId });
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(UserinfoController), nameof(GetLykkewalletToken), $"clientId = {clientAccount.Id}", ex);
                return StatusCode(500, new { Message = "auth error" });
            }
        }

        [HttpGet("~/getprivatekey")]
        [Authorize(AuthenticationSchemes = OAuthValidationConstants.Schemes.Bearer)]
        public async Task<IActionResult> GetPrivateKey()
        {
            var applicationId = HttpContext.GetApplicationId();

            if (!applicationId.IsValidPartitionOrRowKey())
                return BadRequest("Invalid applicationId");

            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null)
                return BadRequest("Application Id Incorrect!");

            var clientId = User.Identity.GetClientId();
            string encodedPrivateKey = string.Empty;

            if (clientId != null)
            {
                var walletCredential = await _walletCredentialsRepository.GetAsync(clientId);

                return Json(new { EncodedPrivateKey = walletCredential?.EncodedPrivateKey });
            }

            return Json(new { EncodedPrivateKey = encodedPrivateKey });
        }

        private async Task<ClientAccountInformationModel> GetClientByEmailAsync(string email)
        {
            ClientAccountInformationModel client = null;

            try
            {
                client = await _clientAccountClient.GetClientByEmailAndPartnerIdAsync(email, null);
            }
            catch (Exception)
            {
                _log.WriteInfo(nameof(GetKycStatus), email.SanitizeEmail(), "Can't get client info");
            }

            return client;
        }
        
        private async Task<ClientModel> GetClientByIdAsync(string clientId)
        {
            ClientModel client = null;

            try
            {
                client = await _clientAccountClient.GetByIdAsync(clientId);
            }
            catch (Exception)
            {
                _log.WriteInfo(nameof(GetKycStatus), clientId, "Can't get client info");
            }

            return client;
        }
    }
}
