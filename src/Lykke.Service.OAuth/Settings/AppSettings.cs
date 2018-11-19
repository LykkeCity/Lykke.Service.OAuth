using JetBrains.Annotations;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.GoogleAnalyticsWrapper.Client;
using Lykke.Service.OAuth.Settings;
using Lykke.Service.OAuth.Settings.ServiceSettings;
using Lykke.Service.PersonalData.Settings;
using Lykke.Service.Session.Client;
using Lykke.Service.Registration;
using WebAuth.Settings.ClientSettings;
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
        public SessionServiceClientSettings SessionServiceClient { get; set; }
        public ConfirmationCodesServiceClientSettings ConfirmationCodesClient { get; set; }
        public IpGeoLocationServiceClientSettings IpGeoLocationServiceClient { get; set; }
    }
}
