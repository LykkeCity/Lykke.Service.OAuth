using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using Common.Log;
using Core.Extensions;
using Core.ExternalProvider.Exceptions;
using Core.Services;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Lykke.Service.OAuth.ExternalProvider
{
    internal class IroncladCookieAuthenticationEvents : OpenIdConnectEvents
    {
        private readonly ILog _log;
        private readonly ITokenService _tokenService;

        public IroncladCookieAuthenticationEvents(
            ILogFactory logFactory,
            ITokenService tokenService)
        {
            _log = logFactory.CreateLog(this);
            _tokenService = tokenService;
        }

        public override Task RedirectToIdentityProvider(RedirectContext context)
        {
            var acrValuesFromRequest =
                context.Properties.GetProperty(OpenIdConnectConstantsExt.AuthenticationProperties.AcrValues);

            if (!string.IsNullOrEmpty(acrValuesFromRequest))
                context.ProtocolMessage.AcrValues = acrValuesFromRequest;

            return Task.CompletedTask;
        }

        public override async Task RedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            var lykkeToken =
                context.Properties.GetString(OpenIdConnectConstantsExt.AuthenticationProperties.LykkeToken);

            if (string.IsNullOrWhiteSpace(lykkeToken))
                return;

            try
            {
                var tokens = await _tokenService.GetIroncladTokens(lykkeToken);
                var idToken = tokens.IdToken;
                context.ProtocolMessage.IdTokenHint = idToken;

                await _tokenService.RevokeIroncladTokensAsync(tokens);

                await _tokenService.DeleteIroncladTokens(lykkeToken);
            }
            catch (TokenNotFoundException e)
            {
                _log.Warning("Ironclad  logout", e);
            }
        }
    }
}
