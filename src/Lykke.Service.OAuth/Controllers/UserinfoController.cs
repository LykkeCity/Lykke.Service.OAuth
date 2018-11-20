using System;
using System.Threading.Tasks;
using Common.Log;
using Core.Extensions;
using Core.ExternalProvider.Exceptions;
using Core.Services;
using IdentityServer4.AccessTokenValidation;
using Lykke.Common.Log;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.OAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserinfoController : Controller
    {
        private readonly ILog _log;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly ITokenService _tokenService;

        public UserinfoController(
            ILogFactory logFactory,
            IClientSessionsClient clientSessionsClient,
            ITokenService tokenService)
        {
            _log = logFactory.CreateLog(this);
            _clientSessionsClient = clientSessionsClient;
            _tokenService = tokenService;
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

        [HttpGet("~/token/kyc")]
        [Authorize(AuthenticationSchemes = OpenIdConnectConstantsExt.Auth.LykkeScheme)]
        public async Task<IActionResult> GetKycToken()
        {
            try
            {
                var sessionId = User.GetClaimValue(OpenIdConnectConstantsExt.Claims.SessionId);

                var accessToken = await _tokenService.GetIroncladAccessTokenAsync(sessionId);

                return Json(new {Token = accessToken});
            }
            catch (Exception e)
                when (e is ClaimNotFoundException ||
                      e is TokenNotFoundException)
            {
                _log.Warning("Token not found.", e);
                return BadRequest("Token not found.");
            }
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

            return Json(new { Token = sessionId, session.AuthId });
        }
    }
}
