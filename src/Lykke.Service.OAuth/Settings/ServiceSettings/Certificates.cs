using JetBrains.Annotations;

namespace WebAuth.Settings.ServiceSettings
{
    [UsedImplicitly]
    public class Certificates
    {
        public const string ContainerName = "certs";
        public string OpenIdConnectCertName { get; set; }
        public string OpenIdConnectCertPassword { get; set; }
    }
}
