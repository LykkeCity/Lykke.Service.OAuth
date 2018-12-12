using System.Collections.Generic;
using Lykke.SettingsReader.Attributes;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     External identity provider configuration.
    /// </summary>
    public class ExternalIdentityProvider
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string Authority { get; set; }
        public string ResponseType { get; set; }
        [Optional] public IEnumerable<string> Scopes { get; set; }
        [Optional] public string ClientSecret { get; set; }
        [Optional] public string AcrValues { get; set; }
        [Optional] public string CallbackPath { get; set; }
        [Optional] public bool? RequireHttpsMetadata { get; set; }
        [Optional] public bool? GetClaimsFromUserInfoEndpoint { get; set; }
        [Optional] public IEnumerable<string> ValidIssuers { get; set; }
    }
}
