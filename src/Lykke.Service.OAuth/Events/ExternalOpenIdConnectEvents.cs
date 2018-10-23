using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.ExternalProvider;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Lykke.Service.OAuth.Events
{
    /// <summary>
    ///     Class for handling events, raised during authentication through external provider.
    /// </summary>
    internal class ExternalOpenIdConnectEvents : OpenIdConnectEvents
    {
        private readonly IExternalProviderService _externalProviderService;

        public ExternalOpenIdConnectEvents(IExternalProviderService externalProviderService)
        {
            _externalProviderService = externalProviderService;
        }

        public override Task TokenValidated(TokenValidatedContext context)
        {
            // Map claims to our claims, based on configuration.
            var claims = context.Principal.Claims;
            var issuer = claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.Issuer)?.Value;
            var providerId = _externalProviderService.GetProviderId(issuer);
            var claimsMappings = _externalProviderService.GetProviderClaimMapping(providerId);

            if (!(context.Principal.Identity is ClaimsIdentity identity))
                return Task.CompletedTask;

            foreach (var fromToMap in claimsMappings)
            {
                var fromClaim = identity.FindFirst(fromToMap.Key);
                if (fromClaim == null)
                    continue;

                var claimType = fromToMap.Value;
                var claim = new Claim(claimType, fromClaim.Value, fromClaim.ValueType, fromClaim.Issuer);
                identity.AddClaim(claim);
            }

            return base.TokenValidated(context);
        }
    }
}
