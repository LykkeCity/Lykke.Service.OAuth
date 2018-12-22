using System.Security.Policy;
using System.Threading.Tasks;
using Common;
using Core.Extensions;
using Core.ExternalProvider;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;

namespace WebAuth
{
    internal class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly IExternalUserOperator _externalUserOperator;

        public CustomCookieAuthenticationEvents(
            IClientSessionsClient clientSessionsClient,
            IExternalUserOperator externalUserOperator)
        {
            _clientSessionsClient = clientSessionsClient;
            _externalUserOperator = externalUserOperator;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;

            var sessionId = userPrincipal.FindFirst(OpenIdConnectConstantsExt.Claims.SessionId)?.Value;

            if (string.IsNullOrEmpty(sessionId) || await _clientSessionsClient.GetAsync(sessionId) == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme);
            }
        }

        public override async Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            // this parameter added for authentification on login page with PartnerId
            context.Properties.Parameters.TryGetValue(OpenIdConnectConstantsExt.Parameters.PartnerId, out var partnerIdValue);

            var partnerIdString = partnerIdValue as string;

            var savedContext = _externalUserOperator.GetLykkeSignInContext();
          
            var partnerId = string.IsNullOrWhiteSpace(partnerIdString) ? savedContext?.Partnerid : partnerIdString;

            var platform = savedContext?.Platform;

            if (!string.IsNullOrWhiteSpace(partnerId))
                context.RedirectUri = QueryHelpers.AddQueryString(context.RedirectUri, OpenIdConnectConstantsExt.Parameters.PartnerId, partnerId);

            if (!string.IsNullOrEmpty(platform))
            {
                context.RedirectUri = context.RedirectUri.Replace(@"/signin?", $@"/signin/{platform}?");
            }

            await base.RedirectToLogin(context);
        }
    }
}
