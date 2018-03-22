using Lykke.Service.GoogleAnalyticsWrapper.Client;
using Lykke.Service.Kyc.Client;
using Lykke.Service.PersonalData.Settings;
using WebAuth.Settings.ClientSettings;
using WebAuth.Settings.ServiceSettings;
using WebAuth.Settings.SlackNotifications;

namespace WebAuth.Settings
{
    public class AppSettings
    {
        public OAuthSettings OAuth { get; set; }
        public PersonalDataServiceClientSettings PersonalDataServiceSettings { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public ClientAccountSettings ClientAccountServiceClient { get; set; }
        public KycServiceClientSettings KycServiceClient { get; set; }
        public GoogleAnalyticsWrapperServiceClientSettings GaWrapperServiceClient { get; set; }
    }
}
