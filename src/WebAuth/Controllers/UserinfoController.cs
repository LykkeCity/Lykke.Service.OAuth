using System;
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
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly IClientAccountClient _clientAccountClient;


        public UserinfoController(
            ILog log,
            IApplicationRepository applicationRepository,
            IClientSessionsClient clientSessionsClient,
            IWalletCredentialsRepository walletCredentialsRepository,
            IClientAccountClient clientAccountClient)

        {
            _log = log;
            _applicationRepository = applicationRepository;
            _clientSessionsClient = clientSessionsClient;
            _walletCredentialsRepository = walletCredentialsRepository;
            _clientAccountClient = clientAccountClient;
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
        
        private async Task<ClientModel> GetClientByIdAsync(string clientId)
        {
            ClientModel client = null;

            try
            {
                client = await _clientAccountClient.GetByIdAsync(clientId);
            }
            catch (Exception)
            {
                _log.WriteInfo(nameof(GetClientByIdAsync), clientId, "Can't get client info");
            }

            return client;
        }
    }
}
