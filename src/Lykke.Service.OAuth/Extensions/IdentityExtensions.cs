using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using AspNet.Security.OpenIdConnect.Primitives;

namespace WebAuth.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetName(this IIdentity src)
        {
            var claimsIdentity = (ClaimsIdentity)src;
            var claims = claimsIdentity.Claims;

            var claimsList = claims as IList<Claim> ?? claims.ToList();

            return claimsList.FirstOrDefault(c => c.Type == OpenIdConnectConstants.Claims.GivenName)?.Value;
        }
        
        public static string GetClientId(this IIdentity src)
        {
            var claimsIdentity = (ClaimsIdentity)src;
            var claims = claimsIdentity.Claims;

            var claimsList = claims as IList<Claim> ?? claims.ToList();

            return claimsList.FirstOrDefault(c => c.Type == OpenIdConnectConstants.Claims.Subject)?.Value;
        }
    }
}
