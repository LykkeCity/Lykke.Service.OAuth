using System;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Extensions;
using Core.ExternalProvider;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.OAuth.Extensions
{
    public static class IroncladExtensions
    {
        /// <summary>
        ///     Registers the IdentityServer authentication handler.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="ironcladSettings">Identity provider configuration object</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddIronclad(
            this AuthenticationBuilder builder,
            IdentityProviderSettings ironcladSettings)
        {
            return builder.AddOpenIdConnect(
                OpenIdConnectConstantsExt.Auth.IroncladAuthenticationScheme,
                OpenIdConnectConstantsExt.Providers.Ironclad,
                options =>
                {
                    FillOpenIdConnectOptionsForExternalProvider(
                        ironcladSettings,
                        options);
                });
        }

        /// <summary>
        ///     Add dicsovery cache.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="authority">Authority with discovery endpoint.</param>
        /// <returns></returns>
        public static IServiceCollection AddDiscoveryCache(this IServiceCollection serviceCollection, string authority)
        {
            return serviceCollection.AddSingleton<IDiscoveryCache>(r =>
            {
                var factory = r.GetRequiredService<IHttpClientFactory>();
                return new DiscoveryCache(authority, () => factory.CreateClient());
            });
        }

        private static void FillOpenIdConnectOptionsForExternalProvider(
            IdentityProviderSettings ironcladSettings,
            OpenIdConnectOptions options)
        {
            if (ironcladSettings == null) throw new ArgumentNullException(nameof(ironcladSettings));

            // One cookie is used as authentication scheme for all external providers.
            options.SignInScheme = OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme;

            options.DisableTelemetry = true;

            options.ClaimActions.MapAllExcept();

            // Set unique callback path for every provider to eliminate intersection.
            options.CallbackPath = string.IsNullOrWhiteSpace(ironcladSettings.CallbackPath)
                ? $"/signin-oidc-{ironcladSettings.Id}"
                : ironcladSettings.CallbackPath;

            options.Authority = ironcladSettings.Authority;
            options.ClientId = ironcladSettings.ClientId;
            options.ResponseType = ironcladSettings.ResponseType;

            if (!string.IsNullOrWhiteSpace(ironcladSettings.ClientSecret))
                options.ClientSecret = ironcladSettings.ClientSecret;

            if (ironcladSettings.ValidIssuers != null)
                options.TokenValidationParameters.ValidIssuers = ironcladSettings.ValidIssuers;

            if (ironcladSettings.RequireHttpsMetadata.HasValue)
                options.RequireHttpsMetadata = ironcladSettings.RequireHttpsMetadata.Value;

            if (ironcladSettings.GetClaimsFromUserInfoEndpoint.HasValue)
                options.GetClaimsFromUserInfoEndpoint = ironcladSettings.GetClaimsFromUserInfoEndpoint.Value;

            if (ironcladSettings.Scopes != null)
            {
                options.Scope.Clear();
                foreach (var scope in ironcladSettings.Scopes) options.Scope.Add(scope);
            }

            if (!string.IsNullOrWhiteSpace(ironcladSettings.AcrValues))
                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    context.ProtocolMessage.AcrValues = ironcladSettings.AcrValues;
                    return Task.CompletedTask;
                };

            options.SaveTokens = true;
        }
    }
}
