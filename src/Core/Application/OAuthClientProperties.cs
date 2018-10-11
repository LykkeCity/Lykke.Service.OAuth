using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Application
{
    [UsedImplicitly]
    public class OAuthClientProperties
    {
        /// <summary>
        /// Set of allowed authorization flows for client.
        /// </summary>
        public IReadOnlyCollection<AuthorizationFlow> AllowedAuthorizationFlows { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [allow offline access]. Defaults to <c>false</c>.
        ///     Allow or disallow issuing of refresh_tokens.
        /// </summary>
        public bool AllowOfflineAccess { get; set; } = false;

        /// <summary>
        ///     Specifies whether a proof key is required for authorization code based token requests (defaults to <c>false</c>).
        ///     plain PKCE are disallowed by default. This property configures usage of encoded PKCE's.
        /// </summary>
        public bool RequirePkce { get; set; } = false;

        /// <summary>
        ///     If set to false, no client secret is needed to request tokens at the token endpoint (defaults to <c>true</c>)
        /// </summary>
        public bool RequireClientSecret { get; set; } = true;


    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuthorizationFlow
    {
        AuthorizationCode,
        Implicit
    }
}
