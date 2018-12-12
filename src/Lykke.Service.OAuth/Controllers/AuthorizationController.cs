using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Common;
using Core.Application;
using Core.Extensions;
using Core.ExternalProvider;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WebAuth.Managers;
using AuthenticationProperties = Microsoft.AspNetCore.Authentication.AuthenticationProperties;
using OpenIdConnectMessage = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage;

namespace WebAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthorizationController : Controller
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IUserManager _userManager;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly IDataProtector _dataProtector;
        private readonly IExternalProviderService _externalProviderService;



        public AuthorizationController(
            IApplicationRepository applicationRepository,
            IUserManager userManager, 
            IClientSessionsClient clientSessionsClient,
            //TODO:@gafanasiev Move protection and cookie creation to service
            IDataProtectionProvider dataProtectionProvider, 
            IExternalProviderService externalProviderService)
        {
            _applicationRepository = applicationRepository;
            _userManager = userManager;
            _clientSessionsClient = clientSessionsClient;
            _externalProviderService = externalProviderService;
            _dataProtector = dataProtectionProvider.CreateProtector(OpenIdConnectConstantsExt.Protectors.ExternalProviderCookieProtector);
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
        public async Task<IActionResult> Authorize()
        {
            // Note: when a fatal error occurs during the request processing, an OpenID Connect response
            // is prematurely forged and added to the ASP.NET context by OpenIdConnectServerHandler.
            // You can safely remove this part and let ASOS automatically handle the unrecoverable errors
            // by switching ApplicationCanDisplayErrors to false in Startup.cs.
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response != null)
            {
                return View("Error", response);
            }

            // Extract the authorization request from the ASP.NET environment.
            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            // Note: ASOS automatically ensures that an application corresponds to the client_id specified
            // in the authorization request by calling IOpenIdConnectServerProvider.ValidateAuthorizationRequest.
            // In theory, this null check shouldn't be needed, but a race condition could occur if you
            // manually removed the application details from the database after the initial check made by ASOS.
            var application = await _applicationRepository.GetByIdAsync(request.ClientId);
            if (application == null)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription =
                        "Details concerning the calling client application cannot be found in the database"
                });
            }

            var tenent = GetTenant(request);

            if (tenent == OpenIdConnectConstantsExt.Tenants.Ironclad)
            {
                return HandleLykkeAuthorize(request);
            }

            return HandleIroncladAuthorize(request);
        }

        [Authorize]
        [HttpPost("~/connect/authorize/external")]
        [HttpGet("~/connect/authorize/external")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
        public IActionResult AuthorizeExternal()
        {
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response != null)
            {
                return View("Error", response);
            }

            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(User.Identity),
                new AuthenticationProperties(),
                OpenIdConnectServerDefaults.AuthenticationScheme);

            //TODO:@gafanasiev add allowed scopes for client application.
            ticket.SetScopes(new[]
            {
                OpenIdConnectConstants.Scopes.OpenId,
                OpenIdConnectConstants.Scopes.Email,
                OpenIdConnectConstants.Scopes.Profile
            }.Intersect(request.GetScopes()));

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        [Authorize]
        [HttpPost("~/connect/authorize/accept")]
        [HttpGet("~/connect/authorize/accept")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
        public async Task<IActionResult> Accept()
        {
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response != null)
            {
                return View("Error", response);
            }

            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            // Remove the authorization request from the user session.
            if (!string.IsNullOrEmpty(request.RequestId))
            {
                HttpContext.Session.Remove("authorization-request:" + request.RequestId);
            }

            var scopes = request.GetScopes().ToList();
            var claims = HttpContext.User.Claims;

            var identity = _userManager.CreateIdentity(scopes, claims.Where(c => c.Type != OpenIdConnectConstantsExt.Claims.SessionId));

            var application = await _applicationRepository.GetByIdAsync(request.ClientId);
            if (application == null)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription =
                        "Details concerning the calling client application cannot be found in the database"
                });
            }

            var sessionId = HttpContext.User.FindFirst(OpenIdConnectConstantsExt.Claims.SessionId)?.Value;
            if (sessionId == null)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = "Empty SessionId",
                    ErrorDescription = "Unable to find session id in the calling context"
                });
            }

            identity.Actor = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);

            identity.Actor.AddClaim(ClaimTypes.NameIdentifier, application.ApplicationId);

            identity.Actor.AddClaim(ClaimTypes.Name, application.DisplayName,
                OpenIdConnectConstants.Destinations.AccessToken,
                OpenIdConnectConstants.Destinations.IdentityToken);

            identity.AddClaim(OpenIdConnectConstantsExt.Claims.SessionId, sessionId, OpenIdConnectConstants.Destinations.AccessToken);

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                OpenIdConnectServerDefaults.AuthenticationScheme);

            // Set the list of scopes granted to the client application.
            // Note: this sample always grants the "openid", "email" and "profile" scopes
            // when they are requested by the client application: a real world application
            // would probably display a form allowing to select the scopes to grant.
            ticket.SetScopes(new[]
            {
                OpenIdConnectConstants.Scopes.OpenId,
                OpenIdConnectConstants.Scopes.Email,
                OpenIdConnectConstants.Scopes.Profile,
                OpenIdConnectConstants.Scopes.OfflineAccess
            }.Intersect(request.GetScopes()));

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        [Authorize]
        [HttpPost("~/connect/authorize/deny")]
        [ValidateAntiForgeryToken]
        public IActionResult Deny(CancellationToken cancellationToken)
        {
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response != null)
            {
                return View("Error", response);
            }

            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            // Remove the authorization request from the user session.
            if (!string.IsNullOrEmpty(request.RequestId))
            {
                HttpContext.Session.Remove("authorization-request:" + request.RequestId);
            }

            // Notify ASOS that the authorization grant has been denied by the resource owner.
            // Note: OpenIdConnectServerHandler will automatically take care of redirecting
            // the user agent to the client application using the appropriate response_mode.
            return Forbid(OpenIdConnectServerDefaults.AuthenticationScheme);
        }

        [HttpGet("~/connect/logout")]
        public ActionResult Logout(CancellationToken cancellationToken)
        {
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response != null)
            {
                return View("Error", response);
            }

            OpenIdConnectRequest request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            return View("Logout", request);
        }

        [HttpPost("~/connect/logout")]
        public async Task<IActionResult> Logout()
        {
            var sessionId = User.FindFirst(OpenIdConnectConstantsExt.Claims.SessionId)?.Value;
            if (sessionId != null)
            {
                await _clientSessionsClient.DeleteSessionIfExistsAsync(sessionId);
            }
            //TODO:@gafanasiev Fix bug with logout 
            return SignOut(OpenIdConnectServerDefaults.AuthenticationScheme);
        }

        private IActionResult HandleIroncladAuthorize(OpenIdConnectRequest request)
        {
            var parameters = request.GetParameters().ToDictionary(item => item.Key, item => item.Value.Value.ToString());

            if (!User.Identities.Any(identity => identity.IsAuthenticated))
            {
                var externalRedirectUrl = QueryHelpers.AddQueryString(Url.Action("AuthorizeExternal"), parameters);

                var properties = new AuthenticationProperties
                {
                    RedirectUri = Url.Action("AfterExternalLoginCallback", "External"),
                    // We need to save original redirectUrl, later get it in AfterExternalLoginCallback and redirect back to it.
                    Items = { { OpenIdConnectConstantsExt.Parameters.AfterExternalLoginCallback, externalRedirectUrl } }
                };

                return Challenge(properties, OpenIdConnectConstantsExt.Auth.IroncladAuthenticationScheme);
            }

            var acceptUri = Url.Action("AuthorizeExternal");

            var redirectUrl = QueryHelpers.AddQueryString(acceptUri, parameters);

            return LocalRedirect(redirectUrl);
        }

        private IActionResult HandleLykkeAuthorize(OpenIdConnectRequest request)
        {
            var parameters = request.GetParameters()
                .ToDictionary(item => item.Key, item => item.Value.Value.ToString());

            string redirectUrl;

            if (!User.Identities.Any(identity => identity.IsAuthenticated))
            {
                var identifier = Guid.NewGuid().ToString();

                // Store the authorization request in the user session.
                HttpContext.Session.Set("authorization-request:" + identifier, request.ToJson().ToUtf8Bytes());
                parameters.Add("request_id", identifier);

                redirectUrl = QueryHelpers.AddQueryString(nameof(Authorize), parameters);

                // this parameter added for authentification on login page with PartnerId
                parameters.TryGetValue(CommonConstants.PartnerIdParameter, out var partnerId);

                var authenticationProperties = new AuthenticationProperties
                {
                    RedirectUri = Url.Action(redirectUrl)
                };

                if (!string.IsNullOrWhiteSpace(partnerId))
                    authenticationProperties.Parameters.Add(CommonConstants.PartnerIdParameter, partnerId);

                return Challenge(authenticationProperties);
            }

            SaveLykkeUserIdAfterExternalLogin();

            var acceptUri = Url.Action("Accept");
            redirectUrl = QueryHelpers.AddQueryString(acceptUri, parameters);

            return Redirect(redirectUrl);
        }

        private string GetTenant(OpenIdConnectRequest request)
        {
            if (request.AcrValues == null) return string.Empty;

            var acrValuesJson = JObject.Parse(request.AcrValues);

            var jToken = acrValuesJson.GetValue(OpenIdConnectConstantsExt.Parameters.Tenant);

            return jToken.Type == JTokenType.String ? jToken.Value<string>() : string.Empty;
        }

        private async Task SaveLykkeUserIdAfterExternalLogin()
        {
            var userId = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId)) return;

            var guid = await _externalProviderService.SaveLykkeUserIdForExternalLoginAsync(userId, TimeSpan.FromMinutes(3));

            // TODO:@gafanasiev check if this supports multiple instances.
            var protectedGuid = _dataProtector.Protect(guid);

            HttpContext.Response.Cookies.Append(OpenIdConnectConstantsExt.Cookies.TemporaryUserIdCookie, protectedGuid, new CookieOptions
            {
                HttpOnly = true,
                MaxAge = TimeSpan.FromMinutes(1)
                //TODO:@gafanasiev uncomment in production
                //Secure = true
            });
        }
    }
}
