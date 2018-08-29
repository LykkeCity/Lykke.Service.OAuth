using System;
using Lykke.SettingsReader.Attributes;

namespace WebAuth.Settings.ServiceSettings
{
    public class CspSettings
    {
        [Optional]
        public string[] ScriptSources { get; set; } = Array.Empty<string>();
        [Optional]
        public string[] StyleSources { get; set; } = Array.Empty<string>();
        [Optional]
        public string[] FontSources { get; set; } = Array.Empty<string>();
    }
}
