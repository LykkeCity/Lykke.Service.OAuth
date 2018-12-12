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
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
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
        private readonly IExternalUserService _externalUserService;
        private readonly IDataProtector _protector;
        private readonly TimeSpan _mobileSessionLifetime;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly ITokenService _tokenService;

        private static readonly OpenIdConnectMessage AuthenticationError = new OpenIdConnectMessage
        {
            Error = OpenIdConnectConstants.Errors.ServerError,
            ErrorDescription = "Authentication error"
        };

        public ExternalController(
            ILogFactory logFactory,
            IClientSessionsClient clientSessionsClient,
            //TODO:@gafanasiev Move protection and cookie creation to service
            IDataProtectionProvider dataProtectionProvider,
            IExternalUserService externalUserService, 
            IClientAccountClient clientAccountClient, 
            ITokenService tokenService)
        {
            _log = logFactory.CreateLog(this);
            _clientSessionsClient = clientSessionsClient;
            _externalUserService = externalUserService;
            _clientAccountClient = clientAccountClient;
            _tokenService = tokenService;
            _protector =
                dataProtectionProvider.CreateProtector(
                    OpenIdConnectConstantsExt.Protectors.ExternalProviderCookieProtector);
            _mobileSessionLifetime = TimeSpan.FromDays(30);
        }

        /// <summary>
        ///     Post processing of external authentication
        /// </summary>
        [HttpGet("login-callback")]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            // Read external identity from the temporary cookie.
            var authenticateResult =
                await HttpContext.AuthenticateAsync(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme);

            if (authenticateResult == null)
            {
                _log.Warning("No authentication result!");
                return View("Error", AuthenticationError);
            }

            if (!authenticateResult.Succeeded)
            {
                _log.Warning("Authentication failed!", authenticateResult.Failure);
                return View("Error", AuthenticationError);
            }

            // Сheck that redirect url is local.
            authenticateResult.Properties.Items.TryGetValue(
                OpenIdConnectConstantsExt.Parameters.ExternalLoginCallback,
                out var externalLoginReturnUrl);

            if (string.IsNullOrWhiteSpace(externalLoginReturnUrl) || !Url.IsLocalUrl(externalLoginReturnUrl))
            {
                _log.Warning($"External login callback url is invalid: {externalLoginReturnUrl}");
                return View("Error", AuthenticationError);
            }

            var principal = authenticateResult.Principal;

            var externalUserId = principal.FindFirst(JwtClaimTypes.Subject)?.Value;
            if (string.IsNullOrEmpty(externalUserId))
            {
                _log.Warning("External User Id is empty");
                return View("Error", AuthenticationError);
            }

            var email = principal.FindFirst(JwtClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                _log.Warning("Email is empty");
                return View("Error", AuthenticationError);
            }

            //TODO:@gafanasiev change idp claim to constant from somwhere.
            var idp = principal.FindFirst("http://schemas.microsoft.com/identity/claims/identityprovider")?.Value;
            if (string.IsNullOrEmpty(idp))
            {
                _log.Warning("Idp is empty");
                return View("Error", AuthenticationError);
            }

            var lsub = principal.FindFirst(OpenIdConnectConstantsExt.Claims.Lsub)?.Value;
            if (!string.IsNullOrEmpty(lsub))
            {
                // Check if lykke user exists.
                var lykkeUser = await _clientAccountClient.GetClientByIdAsync(lsub);

                if (lykkeUser == null)
                {
                    _log.Warning($"Lykke user with id:{lsub} does not exist.");
                    return View("Error", AuthenticationError);
                }

                return await SignInUser(lykkeUser, externalLoginReturnUrl);
            }

            if (idp == OpenIdConnectConstantsExt.Providers.Lykke)
            {
                return await HandleLykkeUserLogin(externalUserId, externalLoginReturnUrl);
            }

            return await HandleExternalUserLogin(externalUserId, idp, principal, externalLoginReturnUrl);
        }

        private async Task<IActionResult> SignInUser(
            ClientAccountInformationModel lykkeUser, 
            string externalLoginReturnUrl)
        {
            var lykkeUserId = lykkeUser.Id;
            
            var email = lykkeUser.Email;

            //TODO:@gafanasiev Think how to get already created session and use it.
            var clientSession =
                await _clientSessionsClient.Authenticate(lykkeUserId, string.Empty, null, null,
                    _mobileSessionLifetime);

            if (clientSession == null)
            {
                _log.Warning($"Unable to create client session! ClientId: {lykkeUserId}");
                return View("Error", AuthenticationError);
            }

            var sessionId = clientSession.SessionToken;

            var refreshToken = await HttpContext.GetTokenAsync(OpenIdConnectConstantsExt.Auth.IroncladAuthenticationScheme,
                OidcConstants.TokenTypes.RefreshToken);

            //TODO:@gafanasiev Get lifetime dynamically
            await _tokenService.SaveIroncladRefreshTokenAsync(sessionId, refreshToken);

            //TODO:@gafanasiev Check how to create claims for user, and how to issue access token here w/o redirect.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, lykkeUserId),
                new Claim(JwtClaimTypes.Email, email),
                new Claim(JwtClaimTypes.Subject, lykkeUserId)
            };

            var identity = new ClaimsIdentity(new GenericIdentity(email, "Token"), claims);

            // Add sessionId only to access token.
            identity.AddClaim(OpenIdConnectConstantsExt.Claims.SessionId, sessionId,
                OpenIdConnectConstants.Destinations.AccessToken);

            // TODO:@gafanasiev Think how to remove this step and authenticate directly with ASOS to issue tokens.
            await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme,
                new ClaimsPrincipal(identity));

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme);

            return LocalRedirect(externalLoginReturnUrl);
        }

        private async Task<IActionResult> HandleLykkeUserLogin(string externalUserId, string externalLoginReturnUrl)
        {
            // Check if external user is already associated with Lykke user.
            var lykkeUserId =
                await _externalUserService.GetAssociatedLykkeUserIdAsync(
                    OpenIdConnectConstantsExt.Providers.Lykke,
                    externalUserId);

            var shouldAssociateUser = false;

            if (string.IsNullOrWhiteSpace(lykkeUserId))
            {
                /* If user authenticated through Lykke OAuth on Ironclad side.
                 * But not associated, get lykkeUserId from cookie and associate user.
                 */
                shouldAssociateUser = true;

                var guidExists = HttpContext.Request.Cookies.TryGetValue(
                    OpenIdConnectConstantsExt.Cookies.TemporaryUserIdCookie,
                    out var protectedGuid);

                // TODO:@gafanasiev Think how to solve this.
                /* Cookie could be empty if user is already authenticated in Ironclad.
                 * This means Ironclad would not redirect to Lykke OAuth but immediately return authenticated user.
                 * Thus cookie would not be created during login.
                 */
                if (!guidExists || string.IsNullOrWhiteSpace(protectedGuid))
                {
                    _log.Warning("Lykke was used to login, but Guid is not saved to cookie.");
                    return View("Error", AuthenticationError);
                }

                var guid = _protector.Unprotect(protectedGuid);

                lykkeUserId = await _externalUserService.GetLykkeUserIdForExternalLoginAsync(guid);

                if (string.IsNullOrWhiteSpace(lykkeUserId))
                {
                    _log.Warning($"Lykke was used to login, but lykkeUserId was not found for guid:{guid}.");
                    return View("Error", AuthenticationError);
                }
            }
            
            // Check if lykke user exists.
            var lykkeUser = await _clientAccountClient.GetClientByIdAsync(lykkeUserId);

            if (lykkeUser == null)
            {
                _log.Warning($"Lykke user with id:{lykkeUserId} does not exist.");
                return View("Error", AuthenticationError);
            }

            if (shouldAssociateUser)
                await _externalUserService.AssociateExternalUserAsync(
                    OpenIdConnectConstantsExt.Providers.Lykke,
                    externalUserId,
                    lykkeUserId);

            await _externalUserService.AddClaimToIroncladUser(externalUserId, OpenIdConnectConstantsExt.Claims.Lsub, lykkeUserId);

            return await SignInUser(lykkeUser, externalLoginReturnUrl);
        }

        private async Task<IActionResult> HandleExternalUserLogin(
            string externalUserId, 
            string identityProvider,
            ClaimsPrincipal principal,
            string externalLoginReturnUrl)
        {
            // Autoprovision user.
            try
            {
                var lykkeUser = await _externalUserService.ProvisionIfNotExistAsync(principal);

                var lykkeUserId = lykkeUser.Id;

                // Associate external user with lykke user.
                await _externalUserService.AssociateExternalUserAsync(
                    identityProvider,
                    externalUserId,
                    lykkeUserId);

                await _externalUserService.AddClaimToIroncladUser(externalUserId, OpenIdConnectConstantsExt.Claims.Lsub, lykkeUserId);

                //TODO:@gafanasiev Change account.Email to 
                return await SignInUser(lykkeUser, externalLoginReturnUrl);
            }
            catch (Exception e) when (
                e is ExternalProviderEmailNotVerifiedException ||
                e is ExternalProviderPhoneNotVerifiedException ||
                e is ExternalProviderClaimNotFoundException ||
                e is UserAutoprovisionFailedException)
            {
                _log.Warning(e.Message);
                return View("Error", AuthenticationError);
            }
        }
    }
}
