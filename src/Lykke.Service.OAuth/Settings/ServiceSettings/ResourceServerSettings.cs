using JetBrains.Annotations;

namespace WebAuth.Settings.ServiceSettings
{
    [UsedImplicitly]
    public class ResourceServerSettings
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string AppSecret { get; set; }
    }
}
