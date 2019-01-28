using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Core.ExternalProvider;
using Core.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class UserSession : IUserSession
    {
        private static readonly string RedisPrefix = "OAuth:UserSessions";
        private static readonly string CookieName = "UserSession";
        private const string IroncladLoginSessionProtector = "IroncladLoginSessionProtector";
        
        private readonly IDatabase _database;
        private readonly IDataProtector _dataProtector;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISystemClock _clock;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly LifetimeSettings _lifetimeSettings;

        public UserSession(
            IConnectionMultiplexer connectionMultiplexer,
            IHttpContextAccessor httpContextAccessor,
            IDataProtectionProvider dataProtectionProvider,
            ISystemClock clock, 
            IHostingEnvironment hostingEnvironment,
            LifetimeSettings lifetimeSettings)
        {
            _database = connectionMultiplexer.GetDatabase();
            _httpContextAccessor = httpContextAccessor;
            _clock = clock;
            _hostingEnvironment = hostingEnvironment;
            _lifetimeSettings = lifetimeSettings;
            _dataProtector =
                dataProtectionProvider.CreateProtector(IroncladLoginSessionProtector);
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            IDictionary<string, string> dict;

            string id;

            if (IsCookieExist())
            {
                id = GetIdFromCookie();

                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new InvalidOperationException("Id should exist in cookie!");
                }

                dict = await GetSessionDataAsync() ?? new Dictionary<string, string>();
            }
            else
            {
                id = GenerateId();

                dict = new Dictionary<string, string>();
            }

            dict[key] = JsonConvert.SerializeObject(value);

            await SaveSessionDataAsync(id, dict);
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string key)
        {
            var dict = await GetSessionDataAsync();

            if (dict == null)
            {
                return default;
            }

            if (dict.TryGetValue(key, out var value))
                return JsonConvert.DeserializeObject<T>(value);

            return default;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(string key)
        {
            var dict = await GetSessionDataAsync();

            if (dict == null)
            {
                return;
            }

            dict.Remove(key);

            var id = GetIdFromCookie();

            await SaveSessionDataAsync(id, dict);
        }

        public async Task EndSessionAsync()
        {
            var id = GetIdFromCookie();

            if (!string.IsNullOrWhiteSpace(id))
            {
                await _database.KeyDeleteAsync(id);

                _httpContextAccessor.HttpContext.Response.Cookies.Delete(CookieName);
            }
        }

        private Task SaveSessionDataAsync(string id, IDictionary<string, string> dict)
        {
            var data = SerializeAndProtect(dict);

            CreateCookie(id);

            return _database.StringSetAsync(GetRedisKey(id), data, _lifetimeSettings.IroncladLoginSessionLifetime);
        }
        
        private async Task<IDictionary<string, string>> GetSessionDataAsync()
        {
            if (!IsCookieExist())
                return null;

            var id = GetIdFromCookie();

            if (string.IsNullOrWhiteSpace(id))
                return null;

            var cachedData = await _database.StringGetAsync(GetRedisKey(id));

            if (!cachedData.HasValue)
                return null;

            return DeserializeAndUnprotect<IDictionary<string, string>>(cachedData);
        }

        private void CreateCookie(string id)
        {
            var useHttps = !_hostingEnvironment.IsDevelopment();
            
            var options = new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Expires = _clock.UtcNow.Add(_lifetimeSettings.IroncladLoginSessionLifetime),
                MaxAge = _lifetimeSettings.IroncladLoginSessionLifetime,
                Secure = useHttps,
                //TODO:@gafanasiev not safe, but we need to check if this is the problem for ios.
                SameSite = SameSiteMode.None
            };

            var data = SerializeAndProtect(id);

            _httpContextAccessor.HttpContext.Response.Cookies.Append(CookieName, data, options);
        }

        private bool IsCookieExist()
        {
            return _httpContextAccessor.HttpContext.Request.Cookies.ContainsKey(CookieName);
        }

        private string GetIdFromCookie()
        {
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(CookieName, out var value))
                return DeserializeAndUnprotect<string>(value);

            return null;
        }

        private string SerializeAndProtect<T>(T value)
        {
            var serialized = JsonConvert.SerializeObject(value);

            return _dataProtector.Protect(serialized);
        }

        private T DeserializeAndUnprotect<T>(string value)
        {
            var unprotected = _dataProtector.Unprotect(value);

            return JsonConvert.DeserializeObject<T>(unprotected);
        }

        private string GenerateId()
        {
            return StringUtils.GenerateId();
        }

        private string GetRedisKey(string id)
        {
            return $"{RedisPrefix}:{id}";
        }
    }
}
