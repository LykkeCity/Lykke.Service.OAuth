using Lykke.Service.ConfirmationCodes.Client;
﻿using JetBrains.Annotations;
using Lykke.Service.GoogleAnalyticsWrapper.Client;
using Lykke.Service.PersonalData.Settings;
using WebAuth.Settings.ClientSettings;
using WebAuth.Settings.ServiceSettings;
using WebAuth.Settings.SlackNotifications;
using Lykke.Service.OAuth.Settings;

namespace WebAuth.Settings
{
    [UsedImplicitly]
    public class AppSettings
    {
        public OAuthSettings OAuth { get; set; }
        public PersonalDataServiceClientSettings PersonalDataServiceSettings { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public ClientAccountSettings ClientAccountServiceClient { get; set; }
        public GoogleAnalyticsWrapperServiceClientSettings GaWrapperServiceClient { get; set; }
        public ConfirmationCodesServiceClientSettings ConfirmationCodesServiceClient { get; set; }
        public IpGeoLocationServiceClientSettings IpGeoLocationServiceClient { get; set; }
    }
}
