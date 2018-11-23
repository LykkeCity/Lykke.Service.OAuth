using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Common.Log;
using Core.Application;
using Core.Bitcoin;
using Core.Extensions;
using IdentityServer4.AccessTokenValidation;
using Lykke.Service.OAuth.Providers;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Models;

namespace Lykke.Service.OAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserinfoController : Controller
    {
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly IKycTokenProvider _kycTokenProvider;


        public UserinfoController(
            IClientSessionsClient clientSessionsClient,
            IKycTokenProvider kycTokenProvider
            )
        {
            _kycTokenProvider = kycTokenProvider;
            _clientSessionsClient = clientSessionsClient;
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

        [HttpGet("~/getkyctoken")]
        [Authorize(AuthenticationSchemes = OpenIdConnectConstantsExt.Auth.LykkeScheme)]
        public async Task<IActionResult> GetKycToken()
        {
            return Json(await _kycTokenProvider.GetKycTokenAsync());
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

            return Json(new { Token = sessionId, session.AuthId });
        }
    }
}
