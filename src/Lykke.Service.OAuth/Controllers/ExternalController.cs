using System;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Common.Log;
using Core.Extensions;
using Core.ExternalProvider;
using Core.ExternalProvider.Exceptions;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIdConnectMessage = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage;

namespace Lykke.Service.OAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("external")]
    public class ExternalController : Controller
    {
        private readonly ILog _log;
        private readonly IExternalUserOperator _externalUserOperator;

        private static readonly OpenIdConnectMessage AuthenticationError = new OpenIdConnectMessage
        {
            Error = OpenIdConnectConstants.Errors.ServerError,
            ErrorDescription = "Authentication error"
        };

        public ExternalController(
            ILogFactory logFactory,
            IExternalUserOperator externalUserOperator)
        {
            _log = logFactory.CreateLog(this);
            _externalUserOperator = externalUserOperator;
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
                var authenticateResult =
                    await HttpContext.AuthenticateAsync(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme);

                var externalLoginReturnUrl = GetRedirectUrl(authenticateResult);

                var ironcladUser = await _externalUserOperator.GetCurrentUserAsync(authenticateResult);

                var lykkeUserAuthenticationContext = await _externalUserOperator.AuthenticateAsync(ironcladUser);

                await _externalUserOperator.SignInAsync(lykkeUserAuthenticationContext);

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

        private string GetRedirectUrl(AuthenticateResult authenticateResult)
        {
            authenticateResult.Properties.Items.TryGetValue(
                OpenIdConnectConstantsExt.AuthenticationProperties.ExternalLoginRedirectUrl,
                out var externalLoginReturnUrl);

            if (!Url.IsLocalUrl(externalLoginReturnUrl))
                throw new AuthenticationException(
                    $"External login callback url is invalid: {externalLoginReturnUrl}");

            return externalLoginReturnUrl;
        }
    }
}
