using System;

namespace WebAuth.Settings.ServiceSettings
{
    public class CorsSettings
    {
        public string[] Origins { get; set; } = Array.Empty<string>();
    }
}
