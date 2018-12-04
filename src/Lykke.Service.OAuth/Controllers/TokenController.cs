using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Server;
using Common.Log;
using Core.Extensions;
using Core.Services;
using Lykke.Common.Log;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.OAuth.Controllers
{
    public class TokenController : Controller
    {
        private readonly ILog _log;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly ITokenService _tokenService;

        public TokenController(
            ILogFactory logFactory,
            IClientSessionsClient clientSessionsClient,
            ITokenService tokenService)

        {
            _log = logFactory.CreateLog(this);
            _clientSessionsClient = clientSessionsClient;
            _tokenService = tokenService;
        }

        [HttpGet("~/getkyctoken")]
        [Authorize(AuthenticationSchemes = OpenIdConnectServerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetKycToken()
        {
            var sessionId = User.FindFirst(OpenIdConnectConstantsExt.Claims.SessionId)?.Value;

            if (sessionId == null)
                return BadRequest("Session id is empty");

            var session = await _clientSessionsClient.GetAsync(sessionId);

            if (session == null)
                return NotFound("Session not found.");

            //TODO:@gafanasiev Add try catch here, to catch for Refresh/Access token not found exceptions.
            var accessToken = await _tokenService.GetIroncladAccessTokenAsync(session.SessionToken);

            return Json(new {Token = accessToken});
        }
    }
}
