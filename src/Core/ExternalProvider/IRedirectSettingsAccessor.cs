using Core.ExternalProvider.Settings;

namespace Core.ExternalProvider
{
    public interface IRedirectSettingsAccessor
    {
        RedirectSettings RedirectSettings { get; }
    }
}
