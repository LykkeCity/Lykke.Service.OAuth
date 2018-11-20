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
using IdentityModel;
using Lykke.Common.Log;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
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
        private readonly IExternalProviderService _externalProviderService;
        private readonly IExternalUserService _externalUserService;
        private readonly IDataProtector _protector;
        private readonly TimeSpan _mobileSessionLifetime = TimeSpan.FromDays(3);

        public ExternalController(
            ILogFactory logFactory,
            IClientSessionsClient clientSessionsClient,
            //TODO:@gafanasiev Move protection and cookie creation to service
            IDataProtectionProvider dataProtectionProvider,
            IExternalProviderService externalProviderService,
            IExternalUserService externalUserService)
        {
            _log = logFactory.CreateLog(this);
            _clientSessionsClient = clientSessionsClient;
            _externalProviderService = externalProviderService;
            _externalUserService = externalUserService;
            _protector =
                dataProtectionProvider.CreateProtector(
                    OpenIdConnectConstantsExt.Protectors.ExternalProviderCookieProtector);
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

            var principal = authenticateResult.Principal;
            var externalUserId = principal.FindFirst(JwtClaimTypes.Subject)?.Value;
            var email = principal.FindFirst(JwtClaimTypes.Email)?.Value;

            // Check if external user is already associated with Lykke user.
            var lykkeUserId =
                await _externalUserService.GetAssociatedLykkeUserIdAsync(OpenIdConnectConstantsExt.Providers.Ironclad,
                    externalUserId);

            if (string.IsNullOrEmpty(lykkeUserId))
            {
                HttpContext.Request.Cookies.TryGetValue(OpenIdConnectConstantsExt.Cookies.TemporaryUserIdCookie,
                    out var protectedGuid);
                if (!string.IsNullOrWhiteSpace(protectedGuid))
                {
                    // If user is not associated, but we authenticated through Lykke OAuth on Ironclad side, and have lykkeUserId in cookie.
                    var guid = _protector.Unprotect(protectedGuid);

                    var lykkeUserIdFromAuthentication =
                        await _externalProviderService.GetLykkeUserIdForExternalLoginAsync(guid);

                    lykkeUserId = lykkeUserIdFromAuthentication;

                    // Associate external user with lykke user.
                    await _externalUserService.AssociateExternalUserAsync(OpenIdConnectConstantsExt.Providers.Ironclad,
                        externalUserId, lykkeUserId);
                }
            }

            // If user is not associated or lykke user id is not saved in cookie,
            // Then we should autoprovision user.
            if (string.IsNullOrWhiteSpace(lykkeUserId)) return View("Error", "Here we should autoprovision user.");

            //TODO:@gafanasiev Think how to get already created session and use it.
            var clientSession =
                await _clientSessionsClient.Authenticate(lykkeUserId, string.Empty, null, null,
                    _mobileSessionLifetime);

            if (clientSession == null)
            {
                _log.Warning($"Unable to create client session! ClientId: {lykkeUserId}");
                return View("Error", externalAuthenticationError);
            }

            var sessionId = clientSession.SessionToken;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, lykkeUserId),
                new Claim(JwtClaimTypes.Email, email),
                new Claim(JwtClaimTypes.Subject, lykkeUserId)
            };

            //TODO:@gafanasiev check email for null.
            var identity = new ClaimsIdentity(new GenericIdentity(email, "Token"), claims);

            // Add sessionId only to access token.
            identity.AddClaim(OpenIdConnectConstantsExt.Claims.SessionId, sessionId,
                OpenIdConnectConstants.Destinations.AccessToken);

            await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme,
                new ClaimsPrincipal(identity));

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme);

            return LocalRedirect(afterExternalLoginReturnUrl);
        }
        catch

        private (Exception e) when(
            e is ExternalProviderNotFoundException ||
        e is ExternalProviderEmailNotVerifiedException ||
        e is ExternalProviderPhoneNotVerifiedException ||
        e is ExternalProviderClaimNotFoundException ||
        e is UserAutoprovisionFailedException) {
            _log.Warning(e.Message);
            return View("Error", externalAuthenticationError);
        }
    }
}
