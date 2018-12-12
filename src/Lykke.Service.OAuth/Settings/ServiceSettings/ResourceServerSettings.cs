using System;
using JetBrains.Annotations;

namespace WebAuth.Settings.ServiceSettings
{
    [UsedImplicitly]
    public class ResourceServerSettings
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool EnableCaching { get; set; }
        public string NameClaimType { get; set; }
        public TimeSpan CacheDuration { get; set; }
        public bool SkipTokensWithDots { get; set; }
    }
}
