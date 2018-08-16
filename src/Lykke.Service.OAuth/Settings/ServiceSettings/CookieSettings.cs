using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;
using Microsoft.AspNetCore.Http;

namespace WebAuth.Settings.ServiceSettings
{
    [UsedImplicitly]
    public class CookieSettings
    {
        [Optional]
        public SameSiteMode SameSiteMode { get; set; } = SameSiteMode.Lax;
    }
}