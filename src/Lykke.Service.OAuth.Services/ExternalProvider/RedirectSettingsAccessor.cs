using Core.ExternalProvider;
using Core.ExternalProvider.Settings;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class RedirectSettingsAccessor : IRedirectSettingsAccessor
    {
        public RedirectSettingsAccessor(RedirectSettings redirectSettings)
        {
            RedirectSettings = redirectSettings;
        }

        public RedirectSettings RedirectSettings { get; }
    }
}
