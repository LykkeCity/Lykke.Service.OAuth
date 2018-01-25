using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using Common;
using Common.Log;
using Core.Application;
using Core.Bitcoin;
using Core.Clients;
using Core.Kyc;
using Lykke.Service.Session;
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
        private readonly IKycRepository _kycRepository;
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly IClientsSessionsRepository _clientSessionsClient;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;

        public UserinfoController(
            ILog log,
            IApplicationRepository applicationRepository,
            IKycRepository kycRepository,
            IClientAccountsRepository clientAccountsRepository,
            IClientsSessionsRepository clientSessionsClient,
            IWalletCredentialsRepository walletCredentialsRepository)
        {
            _log = log;
            _applicationRepository = applicationRepository;
            _kycRepository = kycRepository;
            _clientAccountsRepository = clientAccountsRepository;
            _clientSessionsClient = clientSessionsClient;
            _walletCredentialsRepository = walletCredentialsRepository;
        }

        [Authorize(ActiveAuthenticationSchemes = OpenIdConnectServerDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo")]
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

            if (!applicationId.IsValidRowKey())
                return BadRequest("Invalid applicationId");

            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null)
                return BadRequest("Application Id Incorrect!");

            var client = await _clientAccountsRepository.GetByEmailAsync(email);

            if (client == null)
                return NotFound("Client not found!");

            var kycStatus = await _kycRepository.GetKycStatusAsync(client.Id);
            return Json(kycStatus.ToString());
        }

        [HttpGet("~/getidbyemail")]
        public async Task<IActionResult> GetIdByEmail(string email)
        {
            if (!email.IsValidEmailAndRowKey())
                return BadRequest("Invalid email");

            var applicationId = HttpContext.GetApplicationId();

            if (!applicationId.IsValidRowKey())
                return BadRequest("Invalid applicationId");

            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null)
                return BadRequest("Application Id Incorrect!");

            var client = await _clientAccountsRepository.GetByEmailAsync(email);

            if (client == null)
                return NotFound("Client not found!");

            return Json(client.Id);
        }

        [HttpGet("~/getemailbyid")]
        public async Task<IActionResult> GetEmailById(string id)
        {
            if (!id.IsValidRowKey() && !id.IsGuid())
                return BadRequest("Invalid id");

            var applicationId = HttpContext.GetApplicationId();

            if (!applicationId.IsValidRowKey())
                return BadRequest("Invalid applicationId");

            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null)
                return BadRequest("Application Id Incorrect!");

            var client = await _clientAccountsRepository.GetByIdAsync(id);

            if (client == null)
                return NotFound("Client not found!");

            return Json(client.Email);
        }

        [HttpGet("~/getlykkewallettoken")]
        public async Task<IActionResult> GetLykkewalletToken()
        {
            var applicationId = HttpContext.GetApplicationId();

            if (!applicationId.IsValidRowKey())
                return BadRequest("Invalid applicationId");

            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null)
                return BadRequest("Application Id Incorrect!");

            var clientId = User.GetClaim(ClaimTypes.NameIdentifier);

            if (clientId == null)
                return NotFound("Can't get clientId from claims");

            var clientAccount = await _clientAccountsRepository.GetByIdAsync(clientId);

            if (clientAccount == null)
                return NotFound("Client not found");

            try
            {
                var authResult = await _clientSessionsClient.Authenticate(clientAccount.Id, "oauth server");
                return Json(new { Token = authResult.SessionToken });
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(UserinfoController), nameof(GetLykkewalletToken), $"clientId = {clientAccount.Id}", ex);
                return StatusCode(500, new { Message = "auth error" });
            }
        }

        [HttpGet("~/getprivatekey")]
        public async Task<IActionResult> GetPrivateKey()
        {
            var applicationId = HttpContext.GetApplicationId();

            if (!applicationId.IsValidRowKey())
                return BadRequest("Invalid applicationId");

            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null)
                return BadRequest("Application Id Incorrect!");

            var clientId = User.GetClaim(ClaimTypes.NameIdentifier);
            string encodedPrivateKey = string.Empty;

            if (clientId != null)
            {
                var walletCredential = await _walletCredentialsRepository.GetAsync(clientId);

                return Json(new { EncodedPrivateKey = walletCredential?.EncodedPrivateKey });
            }

            return Json(new { EncodedPrivateKey = encodedPrivateKey });
        }
    }
}
