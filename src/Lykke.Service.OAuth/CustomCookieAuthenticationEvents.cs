using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using Common.Log;
using Core.Extensions;
using Core.ExternalProvider.Exceptions;
using Core.Services;
using Lykke.Common.Log;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;

namespace WebAuth
{
    internal class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly ILog _log;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly ITokenService _tokenService;

        public CustomCookieAuthenticationEvents(
            ILogFactory logFactory,
            IClientSessionsClient clientSessionsClient, 
            ITokenService tokenService)
        {
            _log = logFactory.CreateLog(this);
            _clientSessionsClient = clientSessionsClient;
            _tokenService = tokenService;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;

            var lykkeToken = userPrincipal.GetClaim(OpenIdConnectConstantsExt.Claims.SessionId);

            var isInvalidSession = false;

            if (string.IsNullOrWhiteSpace(lykkeToken))
            {
                isInvalidSession = true;
                await context.HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme);
            }
            else
            {
                var activeSession = await _clientSessionsClient.GetAsync(lykkeToken);
                if (activeSession == null)
                {
                    isInvalidSession = true;

                    try
                    {
                        var tokens = await _tokenService.GetIroncladTokens(lykkeToken);
                        await _tokenService.RevokeIroncladTokensAsync(tokens);
                        await _tokenService.DeleteIroncladTokens(lykkeToken);
                    }
                    catch (TokenNotFoundException e)
                    {
                        _log.Warning("Tokens for inactive session not found.", e);
                    }
                }
            }

            if (isInvalidSession)
            {
                context.RejectPrincipal();
            }
        }

        public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            // this parameter added for authentification on login page with PartnerId
            context.Properties.Parameters.TryGetValue(OpenIdConnectConstantsExt.Parameters.PartnerId, out var partnerIdValue);

            var partnerId = partnerIdValue as string;
         
            if (!string.IsNullOrWhiteSpace(partnerId))
                context.RedirectUri = QueryHelpers.AddQueryString(context.RedirectUri, OpenIdConnectConstantsExt.Parameters.PartnerId, partnerId);

            return base.RedirectToLogin(context);
        }
    }
}
