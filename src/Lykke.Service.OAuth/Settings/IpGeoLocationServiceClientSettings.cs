using Lykke.SettingsReader.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.OAuth.Settings
{
    public class IpGeoLocationServiceClientSettings
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
