using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Common.Log;
using Core.Extensions;
using Core.ExternalProvider;
using Core.ExternalProvider.Exceptions;
using Core.Services;
using IdentityModel;
using Lykke.Common.Log;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIdConnectMessage = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage;

namespace Lykke.Service.OAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("external")]
    public class ExternalController : Controller
    {
        private readonly ILog _log;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly IExternalUserService _externalUserService;
        private readonly TimeSpan _mobileSessionLifetime;
        private readonly ITokenService _tokenService;

        private static readonly OpenIdConnectMessage AuthenticationError = new OpenIdConnectMessage
        {
            Error = OpenIdConnectConstants.Errors.ServerError,
            ErrorDescription = "Authentication error"
        };

        public ExternalController(
            ILogFactory logFactory,
            IClientSessionsClient clientSessionsClient,
            IExternalUserService externalUserService,
            ITokenService tokenService)
        {
            _log = logFactory.CreateLog(this);
            _clientSessionsClient = clientSessionsClient;
            _externalUserService = externalUserService;
            _tokenService = tokenService;
        }

        /// <summary>
        ///     Post processing of external authentication
        /// </summary>
        [HttpGet("login-callback")]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            try
            {
                // Read external identity from the temporary cookie.
                var authenticateResult =
                    await HttpContext.AuthenticateAsync(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme);

                if (authenticateResult == null)
                    throw new AuthenticationException("No authentication result!");

                if (!authenticateResult.Succeeded)
                    throw new AuthenticationException("Authentication failed!", authenticateResult.Failure);

                // Сheck that redirect url is local.
                authenticateResult.Properties.Items.TryGetValue(
                    OpenIdConnectConstantsExt.Parameters.ExternalLoginCallback,
                    out var externalLoginReturnUrl);

                if (!Url.IsLocalUrl(externalLoginReturnUrl))
                    throw new AuthenticationException(
                        $"External login callback url is invalid: {externalLoginReturnUrl}");

                var principal = authenticateResult.Principal;

                var lykkeUser = await _externalUserService.HandleExternalUserLogin(principal);

                await SignInUser(lykkeUser);

                return LocalRedirect(externalLoginReturnUrl);
            }
            catch (Exception e) when (
                e is AuthenticationException ||
                e is ClaimNotFoundException ||
                e is AutoprovisionException)
            {
                _log.Warning(e.Message);
                return View("Error", AuthenticationError);
            }
        }

        private async Task SignInUser(LykkeUserAuthenticationContext context)
        {
            //TODO:@gafanasiev Check how to create claims for user, and how to issue access token here w/o redirect.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, context.UserId),
                new Claim(JwtClaimTypes.Email, context.Email),
                new Claim(JwtClaimTypes.Subject, context.UserId)
            };

            var identity = new ClaimsIdentity(new GenericIdentity(context.Email, "Token"), claims);

            // Add sessionId only to access token.
            identity.AddClaim(OpenIdConnectConstantsExt.Claims.SessionId, context.SessionId,
                OpenIdConnectConstants.Destinations.AccessToken);

            // TODO:@gafanasiev Think how to remove this step and authenticate directly with ASOS to issue tokens.
            await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme,
                new ClaimsPrincipal(identity));

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme);
        }
    }
}
