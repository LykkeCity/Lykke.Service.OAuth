using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Common.Log;
using Core.Extensions;
using Core.ExternalProvider;
using Core.ExternalProvider.Exceptions;
using Core.Services;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Extensions;
using WebAuth.Managers;
using OpenIdConnectMessage = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage;

namespace Lykke.Service.OAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("external")]
    public class ExternalController : Controller
    {
        private readonly ILog _log;
        private readonly IExternalUserOperator _externalUserOperator;
        private readonly ITokenService _tokenService;
        private readonly IUserManager _userManager;

        private static readonly OpenIdConnectMessage AuthenticationError = new OpenIdConnectMessage
        {
            Error = OpenIdConnectConstants.Errors.ServerError,
            ErrorDescription = "Authentication error"
        };


        public ExternalController(
            ILogFactory logFactory,
            IExternalUserOperator externalUserOperator,
            ITokenService tokenService,
            IUserManager userManager)
        {
            _log = logFactory.CreateLog(this);
            _externalUserOperator = externalUserOperator;
            _tokenService = tokenService;
            _userManager = userManager;
        }

        /// <summary>
        ///     Post processing of external authentication
        /// </summary>
        [HttpGet("login-callback")]
        [Authorize(AuthenticationSchemes = OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme)]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            try
            {
                var ironcladPrincipal = await HttpContext.GetIroncladPrincipalAsync();

                var ironcladUser = _userManager.IroncladUserFromIdentity(ironcladPrincipal.Identity as ClaimsIdentity);

                var lykkeUser =
                    await _externalUserOperator.ProvisionIfNotExistAsync(ironcladUser, ironcladPrincipal.Claims);

                var lykkeUserAuthenticationContext = await _externalUserOperator.CreateLykkeSessionAsync(lykkeUser);

                var ironcladRefreshToken = await HttpContext.GetIroncladRefreshTokenAsync();

                await _tokenService.SaveIroncladRefreshTokenAsync(lykkeUserAuthenticationContext.SessionId,
                    ironcladRefreshToken);

                await _externalUserOperator.AssociateIroncladUserAsync(lykkeUser, ironcladUser);

                var lykkeIdentity = _userManager.CreateUserIdentity(lykkeUserAuthenticationContext);

                await HttpContext.SignInAsLykkeUserAsync(lykkeIdentity);

                await HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth
                    .ExternalAuthenticationScheme); 

                await _externalUserOperator.EndUserSessionAsync();

                var externalLoginReturnUrl = await HttpContext.GetIroncladExternalRedirectUrlAsync();

                return LocalRedirect(externalLoginReturnUrl);
            }
            catch (Exception e) when (
                e is AuthenticationException ||
                e is AutoprovisionException)
            {
                _log.Warning(e.Message);
                return View("Error", AuthenticationError);
            }
        }

        /// <summary>
        ///     Post processing of lykke authentication through ironclad
        /// </summary>
        [HttpGet("lykke-login-callback")]
        [Authorize(AuthenticationSchemes = OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme)]
        public async Task<IActionResult> LykkeLoginCallback()
        {
            try
            {
                var ironcladPrincipal = await HttpContext.GetIroncladPrincipalAsync();

                var ironcladUser = _userManager.IroncladUserFromIdentity(ironcladPrincipal.Identity as ClaimsIdentity);

                var lykkeUserId = await _externalUserOperator.GetTempLykkeUserIdAsync(); 

                //TODO: @gafanasiev change to faster way (cache user in redis or cookie).
                var lykkeUser = await _userManager.GetLykkeUserAsync(lykkeUserId);

                var lykkeUserAuthenticationContext = await _externalUserOperator.CreateLykkeSessionAsync(lykkeUser);

                var sessionId = lykkeUserAuthenticationContext.SessionId;
                
                var lykkeIdentity = _userManager.CreateUserIdentity(lykkeUserAuthenticationContext);
                
                var ironcladRefreshToken = await HttpContext.GetIroncladRefreshTokenAsync();

                // TODO:@gafanasiev Save access token, save id_token
                await _tokenService.SaveIroncladRefreshTokenAsync(sessionId, ironcladRefreshToken);

                await _externalUserOperator.AssociateIroncladUserAsync(lykkeUser, ironcladUser);

                await HttpContext.SignInAsLykkeUserAsync(lykkeIdentity);

                await _externalUserOperator.ClearTempLykkeUserIdAsync();

                await HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth
                    .ExternalAuthenticationScheme);

                await _externalUserOperator.EndUserSessionAsync();

                var externalLoginReturnUrl = await HttpContext.GetIroncladExternalRedirectUrlAsync();
                
                return Redirect(externalLoginReturnUrl);
            }
            catch (Exception e) when (
                e is AuthenticationException ||
                e is AutoprovisionException)
            {
                _log.Warning(e.Message);
                return View("Error", AuthenticationError);
            }
        }
    }
}
