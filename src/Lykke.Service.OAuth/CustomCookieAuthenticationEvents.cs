using System.Threading.Tasks;
using Core.Extensions;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebAuth
{
    internal class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
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
    }
}
