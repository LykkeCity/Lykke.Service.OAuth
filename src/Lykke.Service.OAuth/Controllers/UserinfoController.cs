using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using Common.Log;
using Core.Extensions;
using Core.ExternalProvider.Exceptions;
using Core.Services;
using IdentityServer4.AccessTokenValidation;
using Lykke.Common.Log;
using Lykke.Service.ClientAccount.Client;
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

        private static string _tokenNotFoundMessage = "Token not found.";
        private readonly IClientAccountClient _clientAccountClient;

        public UserinfoController(
            ILogFactory logFactory,
            IClientSessionsClient clientSessionsClient,
            ITokenService tokenService,
            IClientAccountClient clientAccountClient)
        {
            _clientAccountClient = clientAccountClient;
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
            var sessionId = User.GetClaim(OpenIdConnectConstantsExt.Claims.SessionId);

            if (string.IsNullOrEmpty(sessionId))
            {
                _log.Warning("No session id.");
            
                return BadRequest(_tokenNotFoundMessage);
            }

            try
            {
                var tokens = await _tokenService.GetFreshIroncladTokens(sessionId);

                var accessToken = tokens.AccessToken;

                return Json(new {Token = accessToken});
            }
            catch (TokenNotFoundException e)
            {
                _log.Warning(_tokenNotFoundMessage, e);
                return BadRequest(_tokenNotFoundMessage);
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
            {
                _log.Warning($"Session not found when trying to get token, sessionId = {sessionId}");
                return NotFound("Session not found.");
            }

            var clientAccount = await _clientAccountClient.GetByIdAsync(session.ClientId);

            return Json(new { Token = sessionId, session.AuthId, clientAccount.NotificationsId });
        }
    }
}
