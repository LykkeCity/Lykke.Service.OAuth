using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Common;
using Core.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using WebAuth.ActionHandlers;
using WebAuth.Managers;
using WebAuth.Models;
using AuthenticationProperties = Microsoft.AspNetCore.Authentication.AuthenticationProperties;
using OpenIdConnectMessage = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage;

namespace WebAuth.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly AuthorizationActionHandler _authorizationActionHandler;
        private readonly IUserManager _userManager;

        public AuthorizationController(IApplicationRepository applicationRepository, IUserManager userManager,
            AuthorizationActionHandler authorizationActionHandler)
        {
            _applicationRepository = applicationRepository;
            _userManager = userManager;
            _authorizationActionHandler = authorizationActionHandler;
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
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

            // Note: authentication could be theorically enforced at the filter level via AuthorizeAttribute
            // but this authorization endpoint accepts both GET and POST requests while the cookie middleware
            // only uses 302 responses to redirect the user agent to the login page, making it incompatible with POST.
            // To work around this limitation, the OpenID Connect request is automatically saved in the user session and will be
            // restored by AuthorizationProvider.ExtractAuthorizationRequest after the external authentication process has been completed.
            if (!User.Identities.Any(identity => identity.IsAuthenticated))
            {
                var identifier = Guid.NewGuid().ToString();

                var parameters = request.GetParameters().ToList();
                parameters.Add(new KeyValuePair<string, OpenIdConnectParameter>("request_id", new OpenIdConnectParameter(identifier)));

                var dict = parameters.Where(item => item.Value.Value != null).ToDictionary(item => item.Key, pair => pair.Value.Value.ToString());

                // Store the authorization request in the user session.
                HttpContext.Session.Set("authorization-request:" + identifier, Encoding.UTF8.GetBytes(dict.ToJson()));

                var redirectUrl = QueryHelpers.AddQueryString(nameof(Authorize), dict);

                return Challenge(new AuthenticationProperties
                {
                    RedirectUri = Url.Action(redirectUrl)
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

            if (await _authorizationActionHandler.IsTrustedApplicationAsync(_userManager.GetCurrentUserId(), application.ApplicationId))
            {
                var parameters = request.GetParameters().ToList();
                var acceptUri = Url.Action("Accept");
                var redirectUrl = QueryHelpers.AddQueryString(acceptUri, parameters.ToDictionary(item => item.Key, pair => pair.Value.Value.ToString()));

                return Redirect(redirectUrl);
            }

            return View("Authorize", new AuthorizeViewModel
            {
                ApplicationName = application.DisplayName,
                Parameters = request.GetParameters().ToDictionary(item => item.Key, pair => pair.Value.Value.ToString()),
                Scopes = request.GetScopes(),
                Scope = request.Scope
            });
        }

        [Authorize]
        [HttpPost("~/connect/authorize/accept")]
        [HttpGet("~/connect/authorize/accept")]
        public async Task<IActionResult> Accept(CancellationToken cancellationToken)
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

            var identity = _userManager.CreateIdentity(scopes, claims);

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

            await _authorizationActionHandler.AddTrustedApplication(_userManager.GetCurrentUserId(), application.ApplicationId);

            identity.Actor = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);

            identity.Actor.AddClaim(ClaimTypes.NameIdentifier, application.ApplicationId);

            identity.Actor.AddClaim(ClaimTypes.Name, application.DisplayName,
                OpenIdConnectConstants.Destinations.AccessToken,
                OpenIdConnectConstants.Destinations.IdentityToken);

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

            // Set the resources servers the access token should be issued for.
            ticket.SetResources("resource_server");
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
        public async Task<ActionResult> Logout(CancellationToken cancellationToken)
        {
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response != null)
            {
                return View("Error", response);
            }

            var identity = await HttpContext.AuthenticateAsync(OpenIdConnectServerDefaults.AuthenticationScheme);

            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("Error", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            return View("Logout", Tuple.Create(request, identity));
        }

        [HttpPost("~/connect/logout")]
        public ActionResult Logout()
        {
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectServerDefaults.AuthenticationScheme);
        }
    }
}