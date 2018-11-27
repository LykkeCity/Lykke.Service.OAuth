using System.Collections.Generic;

namespace Lykke.Service.OAuth.Settings.ServiceSettings
{
    public class FeatureToggleSettings
    {
        public IEnumerable<string> DisabledFeatures { get; set; }
    }
}
