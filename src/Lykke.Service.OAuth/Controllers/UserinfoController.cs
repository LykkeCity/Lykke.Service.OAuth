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
using Lykke.Common.Log;
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
            ILogFactory logFactory,
            IApplicationRepository applicationRepository,
            IClientSessionsClient clientSessionsClient,
            IWalletCredentialsRepository walletCredentialsRepository,
            IClientAccountClient clientAccountClient)

        {
            _log = logFactory.CreateLog(this);
            _applicationRepository = applicationRepository;
            _clientSessionsClient = clientSessionsClient;
            _walletCredentialsRepository = walletCredentialsRepository;
            _clientAccountClient = clientAccountClient;
        }

        [HttpGet("~/getlykkewallettoken")]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme, Policy = OpenIdConnectConstantsExt.Policies.OnlyLykkeSignIn)]
        public async Task<IActionResult> GetLykkewalletToken()
        {
            return await GetToken();
        }

        // Do not delete or merge it with getlykkewallettoken. It will break mobiles
        [HttpGet("~/getlykkewallettokenmobile")]
        [Authorize(AuthenticationSchemes = OpenIdConnectConstantsExt.Auth.DefaultScheme, Policy = OpenIdConnectConstantsExt.Policies.OnlyLykkeSignIn)]
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

            if (session == null)
                return NotFound("Session not found.");

            return Json(new { Token = session.SessionToken, session.AuthId });
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
                _log.Info(nameof(GetClientByIdAsync), clientId, "Can't get client info");
            }

            return client;
        }
    }
}
