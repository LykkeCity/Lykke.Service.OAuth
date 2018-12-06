using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;

namespace Lykke.Service.OAuth.Middleware
{
    internal class ClaimsCache
    {
        public class PrincipalCacheItem
        {
            public ClaimsPrincipal ClaimsPrincipal { get; set; }
            public DateTime LastRefresh { get; private set; }

            public static PrincipalCacheItem Create(ClaimsPrincipal src)
            {
                return new PrincipalCacheItem
                {
                    LastRefresh = DateTime.UtcNow,
                    ClaimsPrincipal = src
                };
            }
        }

        private readonly int _secondsToExpire;

        private readonly Dictionary<string, PrincipalCacheItem> _claimsCache =
            new Dictionary<string, PrincipalCacheItem>();

        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();

        public ClaimsCache(int secondsToExpire = 60)
        {
            _secondsToExpire = secondsToExpire;
        }

        public ClaimsPrincipal Get(string token)
        {
            _cacheLock.EnterUpgradeableReadLock();

            try
            {
                if (!_claimsCache.TryGetValue(token, out var result)) return null;

                if ((DateTime.UtcNow - result.LastRefresh).TotalSeconds < _secondsToExpire)
                    return result.ClaimsPrincipal;

                _cacheLock.EnterWriteLock();

                try
                {
                    _claimsCache.Remove(token);

                    return null;
                }
                finally
                {
                    _cacheLock.ExitWriteLock();
                }
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }

        public void Set(string token, ClaimsPrincipal principal)
        {
            _cacheLock.EnterWriteLock();

            try
            {
                if (_claimsCache.ContainsKey(token))
                    _claimsCache[token] = PrincipalCacheItem.Create(principal);
                else
                    _claimsCache.Add(token, PrincipalCacheItem.Create(principal));
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }
    }
}
