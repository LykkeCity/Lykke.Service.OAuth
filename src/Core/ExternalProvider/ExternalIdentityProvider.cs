using System.Collections.Generic;
using JetBrains.Annotations;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     External identity provider configuration.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ExternalIdentityProvider
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
        public string ResponseType { get; set; }
        public string AuthenticationScheme { get; set; }
        public bool GetClaimsFromUserInfoEndpoint { get; set; }
        public string LogoutPath { get; set; }
        public IEnumerable<string> ValidIssuers { get; set; }
        public IEnumerable<string> Scopes { get; set; }
    }
}
