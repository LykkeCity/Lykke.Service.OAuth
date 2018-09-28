using JetBrains.Annotations;
using Lykke.Service.GoogleAnalyticsWrapper.Client;
using Lykke.Service.PersonalData.Settings;
using Lykke.Service.Registration;
using WebAuth.Settings.ClientSettings;
using WebAuth.Settings.ServiceSettings;
using WebAuth.Settings.SlackNotifications;

namespace WebAuth.Settings
{
    [UsedImplicitly]
    public class AppSettings
    {
        public OAuthSettings OAuth { get; set; }
        public PersonalDataServiceClientSettings PersonalDataServiceSettings { get; set; }
        public RegistrationServiceClientSettings RegistrationServiceClient{ get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public ClientAccountSettings ClientAccountServiceClient { get; set; }
        public GoogleAnalyticsWrapperServiceClientSettings GaWrapperServiceClient { get; set; }
    }
}
