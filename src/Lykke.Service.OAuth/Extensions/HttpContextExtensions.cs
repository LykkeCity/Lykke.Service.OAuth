using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Extensions;
using Core.ExternalProvider.Exceptions;
using IdentityModel;
using Lykke.Common.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace WebAuth.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetApplicationId(this HttpContext ctx)
        {
            return ctx.GetHeaderValueAs<string>("application_id");
        }

        public static Task<ClaimsPrincipal> GetIroncladPrincipalAsync(this HttpContext ctx)
        {
            return ctx.GetPrincipalAsync(OpenIdConnectConstantsExt.Auth.IroncladAuthenticationScheme);
        }        
        
        public static async Task<ClaimsPrincipal> GetPrincipalAsync(this HttpContext ctx, string scheme)
        {
            var authenticateResult = await ctx.AuthenticateAsync(scheme);

            if (authenticateResult == null)
                throw new AuthenticationException("No authentication result");

            if (!authenticateResult.Succeeded)
                throw new AuthenticationException("Authentication failed", authenticateResult.Failure);

            return authenticateResult.Principal;
        }

        public static Task<string> GetIroncladRefreshTokenAsync(this HttpContext ctx)
        {
            return ctx.GetTokenAsync(
                OpenIdConnectConstantsExt.Auth.IroncladAuthenticationScheme,
                OidcConstants.TokenTypes.RefreshToken);
        }

        public static Task SignInAsLykkeUserAsync(this HttpContext ctx, ClaimsIdentity lykkeIdentity)
        {
            return ctx.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme,
                new ClaimsPrincipal(lykkeIdentity));
        }
        
        public static async Task<string> GetIroncladExternalRedirectUrlAsync(this HttpContext ctx)
        {
            var authResult = await ctx.AuthenticateAsync(OpenIdConnectConstantsExt.Auth.IroncladAuthenticationScheme);

            authResult.Properties.Items.TryGetValue(
                OpenIdConnectConstantsExt.AuthenticationProperties.ExternalLoginRedirectUrl,
                out var externalLoginReturnUrl);

            return externalLoginReturnUrl;
        }
    }
}
