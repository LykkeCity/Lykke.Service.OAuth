using System;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Common;
using Common.Log;
using Core.Application;
using Core.Bitcoin;
using Core.Extensions;
using IdentityServer4.AccessTokenValidation;
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
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
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
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetLykkewalletToken()
        {
            return await GetToken();
        }

        // Do not delete or merge it with getlykkewallettoken. It will break mobiles
        [HttpGet("~/getlykkewallettokenmobile")]
        [Authorize(AuthenticationSchemes = OpenIdConnectConstantsExt.Auth.DefaultScheme)]
        public async Task<IActionResult> GetLykkeWalletTokenMobile()
        {
            return await GetToken();
        }

        private async Task<IActionResult> GetToken()
        {
            var sessionId = User.FindFirst(OpenIdConnectConstantsExt.Claims.SessionId)?.Value;

            // We are 100% sure that here we have a session id because the request was validated in the retrospection. But...
            if (sessionId == null)
            {
                return BadRequest("Session id is empty");
            }

            var session = await _clientSessionsClient.GetAsync(sessionId);

            return Json(new {Token = sessionId, session.AuthId});
        }


        [HttpGet("~/getprivatekey")]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
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
