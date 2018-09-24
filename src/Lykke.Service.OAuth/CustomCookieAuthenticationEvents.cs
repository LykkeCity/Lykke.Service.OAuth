using System.Threading.Tasks;
using Core.Extensions;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;

namespace WebAuth
{
    internal class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private const string PartnerIdName = "partnerId";
        private readonly IClientSessionsClient _clientSessionsClient;

        public CustomCookieAuthenticationEvents(IClientSessionsClient clientSessionsClient)
        {
            _clientSessionsClient = clientSessionsClient;
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
            context.Properties.Parameters.TryGetValue(PartnerIdName, out var partnerIdValue);

            var partnerId = partnerIdValue as string;

            if (!string.IsNullOrWhiteSpace(partnerId))
                context.RedirectUri = QueryHelpers.AddQueryString(context.RedirectUri, PartnerIdName, partnerId);

            await base.RedirectToLogin(context);
        }
    }
}
