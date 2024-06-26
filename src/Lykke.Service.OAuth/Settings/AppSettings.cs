﻿using JetBrains.Annotations;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.GoogleAnalyticsWrapper.Client;
using Lykke.Service.OAuth.Settings;
using Lykke.Service.PersonalData.Settings;
using Lykke.Service.Session.Client;
using Lykke.Service.Registration;
using WebAuth.Settings.ServiceSettings;
using WebAuth.Settings.SlackNotifications;
using Lykke.Service.Kyc.Client;

namespace WebAuth.Settings
{
    [UsedImplicitly]
    public class AppSettings
    {
        public OAuthSettings OAuth { get; set; }
        public PersonalDataServiceClientSettings PersonalDataServiceSettings { get; set; }
        public RegistrationServiceClientSettings RegistrationServiceClient{ get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public ClientAccountServiceClientSettings ClientAccountServiceClient { get; set; }
        public GoogleAnalyticsWrapperServiceClientSettings GaWrapperServiceClient { get; set; }
        public SessionServiceClientSettings SessionServiceClient { get; set; }
        public ConfirmationCodesServiceClientSettings ConfirmationCodesClient { get; set; }
        public IpGeoLocationServiceClientSettings IpGeoLocationServiceClient { get; set; }
        public KycServiceClientSettings KycServiceClient { get; set; }
    }
}
