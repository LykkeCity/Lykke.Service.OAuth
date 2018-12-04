using System.Security.Claims;
using Core.ExternalProvider.Exceptions;

namespace Core.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetTokenClaim(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            var value = claimsPrincipal.FindFirst(claimType)?.Value;

            if (string.IsNullOrWhiteSpace(value))
                throw new ClaimNotFoundException($"{claimType} not found");

            return value;
        }
    }
}
