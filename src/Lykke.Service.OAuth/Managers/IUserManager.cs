using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.ExternalProvider;

namespace WebAuth.Managers
{
    public interface IUserManager
    {
        ClaimsIdentity CreateIdentity(List<string> scopes, IEnumerable<Claim> claims);

        Task<ClaimsIdentity> CreateUserIdentityAsync(string clientId, string email, string userName,string partnerId, string sessionId, bool? register = null);
        
        ClaimsIdentity CreateUserIdentity(LykkeUserAuthenticationContext context);

        string GetCurrentUserId();

        IroncladUser IroncladUserFromIdentity(ClaimsIdentity identity);

        Task<LykkeUser> GetLykkeUserAsync(string lykkeUserId);

        IEnumerable<Claim> ClaimsFromLykkeUser(LykkeUser lykkeUser);
    }
}
