using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Application
{
    public class ClientApplication : IApplication
    {
        public string ApplicationId { get; private set; }
        public string DisplayName { get; private set; }
        public string RedirectUri { get; private set; }
        public string Secret { get; private set; }
        public string Type { get; private set; }
        public IReadOnlyCollection<GrantType> GrantTypes { get; private set; }

        public IEnumerable<string> Urls => string.IsNullOrEmpty(RedirectUri?.Trim())
            ? Array.Empty<string>()
            : RedirectUri.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToArray();

        public static ClientApplication Create(IApplication src)
        {
            return src == null
                ? null
                : new ClientApplication
                {
                    ApplicationId = src.ApplicationId,
                    DisplayName = src.DisplayName,
                    RedirectUri = src.RedirectUri,
                    Secret = src.Secret,
                    Type = src.Type,
                    GrantTypes = src.GrantTypes
                };
        }
    }

    public class OAuthSettings
    {
        public IReadOnlyCollection<AuthorizationOptions> Options { get; set; }
        public ClientType ClientType { get; set; }

    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuthorizationFlow
    {
        Code,
        Implicit
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GrantType
    {
        AuthorizationCode,
        RefreshToken
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ClientType
    {
        Public,
        Confidential
    }

    public class AuthorizationOptions
    {
        public AuthorizationFlow AuthorizationFlow { get; set; }
        public GrantType GrantType { get; set; }
    }
}
