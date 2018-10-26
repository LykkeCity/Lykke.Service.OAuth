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
using Lykke.Common.Log;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIdConnectMessage = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage;

namespace Lykke.Service.OAuth.Controllers
{
    [Route("external")]
    public class ExternalController : Controller
    {
        private readonly ILog _log;
        private readonly IExternalUserService _externalUserService;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly TimeSpan _mobileSessionLifetime;

        public ExternalController(
            ILogFactory logFactory,
            IExternalUserService externalUserService,
            IClientSessionsClient clientSessionsClient,
            ILifetimeSettingsProvider lifetimeSettingsProvider)
        {
            _log = logFactory.CreateLog(this);
            _externalUserService = externalUserService;
            _clientSessionsClient = clientSessionsClient;
            _mobileSessionLifetime = lifetimeSettingsProvider.GetMobileSessionLifetime();
        }

        /// <summary>
        ///     Post processing of external authentication
        /// </summary>
        [HttpGet("login-callback")]
        public async Task<IActionResult> AfterExternalLoginCallback()
        {
            var externalAuthenticationErrorMessage = "External authentication error";
            var externalAuthenticationError = new OpenIdConnectMessage
            {
                Error = OpenIdConnectConstants.Errors.ServerError,
                ErrorDescription = externalAuthenticationErrorMessage
            };

            // Read external identity from the temporary cookie.
            var authenticateResult =
                await HttpContext.AuthenticateAsync(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme);

            if (authenticateResult == null)
            {
                _log.Warning("No authentication result!");
                return View("Error", externalAuthenticationError);
            }

            if (!authenticateResult.Succeeded)
            {
                _log.Warning(externalAuthenticationErrorMessage, authenticateResult.Failure);
                return View("Error", externalAuthenticationError);
            }

            // Сheck that redirect url is local.
            authenticateResult.Properties.Items.TryGetValue(
                OpenIdConnectConstantsExt.Parameters.AfterExternalLoginCallback,
                out var afterExternalLoginReturnUrl);

            if (string.IsNullOrWhiteSpace(afterExternalLoginReturnUrl) || !Url.IsLocalUrl(afterExternalLoginReturnUrl))
            {
                _log.Warning($"After external login callback url is invalid: {afterExternalLoginReturnUrl}");
                return View("Error", externalAuthenticationError);
            }

            // Autoprovision user.
            var principal = authenticateResult.Principal;

            try
            {
                var account = await _externalUserService.ProvisionIfNotExistAsync(principal);

                var clientId = account.Id;

                //TODO:@gafanasiev Think how to get already created session and use it.
                var clientSession =
                    await _clientSessionsClient.Authenticate(clientId, string.Empty, null, null,
                        _mobileSessionLifetime);

                if (clientSession == null)
                {
                    _log.Warning($"Unable to create client session! ClientId: {account.Id}");
                    return View("Error", externalAuthenticationError);
                }

                var sessionId = clientSession.SessionToken;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, clientId),
                    new Claim(OpenIdConnectConstants.Claims.Email, account.Email),
                    new Claim(OpenIdConnectConstants.Claims.Subject, clientId)
                };

                var identity = new ClaimsIdentity(new GenericIdentity(account.Email, "Token"), claims);

                // Add sessionId only to access token.
                identity.AddClaim(OpenIdConnectConstantsExt.Claims.SessionId, sessionId,
                    OpenIdConnectConstants.Destinations.AccessToken);

                await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme,
                    new ClaimsPrincipal(identity));

                // delete temporary cookie used during external authentication
                await HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme);

                return Redirect(afterExternalLoginReturnUrl);
            }
            catch (Exception e) when (
                e is ExternalProviderNotFoundException ||
                e is ExternalProviderPhoneNotVerifiedException ||
                e is ExternalProviderClaimNotFoundException ||
                e is UserAutoprovisionFailedException)
            {
                _log.Warning(e.Message);
                return View("Error", externalAuthenticationError);
            }
        }
    }
}
