﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAuth.Managers
{
    public interface IUserManager
    {
        ClaimsIdentity CreateIdentity(List<string> scopes, IEnumerable<Claim> claims);

        Task<ClaimsIdentity> CreateUserIdentityAsync(string clientId, string email, string userName,string partnerId, string sessionId, bool? register = null);

        string GetCurrentUserId();
    }
}
