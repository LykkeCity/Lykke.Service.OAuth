using System.Collections.Generic;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     External identity provider configuration.
    /// </summary>
    public class ExternalIdentityProvider
    {
        public string Id { get; set; }
        public IEnumerable<string> ValidIssuers { get; set; }
        public IEnumerable<string> ValidScopes { get; set; }
        public string LogoutPath { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
        public string ResponseType { get; set; }
        public IDictionary<string, string> ClaimsMapping { get; set; }
    }
}
