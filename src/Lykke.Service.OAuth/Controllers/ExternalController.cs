using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Core.Extensions;
using Core.ExternalProvider;
using Core.Services;
using IdentityModel;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OpenIdConnectMessage = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage;

namespace Lykke.Service.OAuth.Controllers
{
    [Route("external")]
    public class ExternalController : Controller
    {
        private readonly IExternalUserService _externalUserService;
        private readonly IExternalProviderService _externalProviderService;
        private readonly IClientSessionsClient _clientSessionsClient;

        private readonly TimeSpan _mobileSessionLifetime;
        
        public ExternalController(
            IExternalUserService externalUserService,
            IExternalProviderService externalProviderService,
            IClientSessionsClient clientSessionsClient,
            ILifetimeSettingsProvider lifetimeSettingsProvider)
        {
            _externalUserService = externalUserService;
            _externalProviderService = externalProviderService;
            _clientSessionsClient = clientSessionsClient;

            _mobileSessionLifetime = lifetimeSettingsProvider.GetMobileSessionLifetime();

        }

        /// <summary>
        ///     Post processing of external authentication
        /// </summary>
        [HttpGet("login-callback")]
        public async Task<IActionResult> AfterExternalLoginCallback()
        {
            // Read external identity from the temporary cookie.
            var authenticateResult =
                await HttpContext.AuthenticateAsync(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme);       
            
            if (authenticateResult?.Succeeded != true)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "External authentication error"
                });
            }

            // Сheck that redirect url is local.
            authenticateResult.Properties.Items.TryGetValue(CommonConstants.AfterExternalLoginReturnUrl,
                out var afterExternalLoginReturnUrl);

            if (string.IsNullOrWhiteSpace(afterExternalLoginReturnUrl) || !Url.IsLocalUrl(afterExternalLoginReturnUrl))
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "Return url after external login is invalid!"
                });
            }

            // Autoprovision user.
            var principal = authenticateResult.Principal;

            var externalUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var issuer = principal.FindFirst(JwtClaimTypes.Issuer)?.Value;
            string externalIdentityProviderId;

            try
            {
                externalIdentityProviderId = _externalProviderService.GetProviderId(issuer);
            }
            catch (ExternalProviderNotFound)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "External provider not found!"
                });
            }

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "Email claim was not provided!"
                });
            }

            var isPhoneVerified = principal.FindFirst(OpenIdConnectConstantsExt.Claims.PhoneNumberVerified)?.Value;

            if (!Convert.ToBoolean(isPhoneVerified))
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "Phone is not verified on provider side!"
                });
            }

            var phone = principal.FindFirst(ClaimTypes.MobilePhone)?.Value;

            var account = await _externalUserService.ProvisionIfNotExistAsync(new ExternalClientProvisionModel
            {
                Email = email,
                ExternalIdentityProviderId = externalIdentityProviderId,
                ExternalUserId = externalUserId,
                Phone = phone
            });

            if (account == null) 
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "Unable to create client account!"
                });

            var clientId = account.Id;

            //TODO:@gafanasiev Think how to get already created session and use it.
            var clientSession =
                await _clientSessionsClient.Authenticate(clientId, string.Empty, null, null, _mobileSessionLifetime);

            if (clientSession == null) 
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "Unable to create client session!"
                });

            var sessionId = clientSession.SessionToken;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, clientId),
                new Claim(OpenIdConnectConstants.Claims.Email, email),
                new Claim(OpenIdConnectConstants.Claims.Subject, clientId),
            };

            var identity = new ClaimsIdentity(new GenericIdentity(email, "Token"), claims);

            // Add sessionId only to access token.
            identity.AddClaim(OpenIdConnectConstantsExt.Claims.SessionId, sessionId, OpenIdConnectConstants.Destinations.AccessToken);

            await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme, new ClaimsPrincipal(identity));

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme);

            return Redirect(afterExternalLoginReturnUrl);
        }
    }
}
