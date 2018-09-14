using Autofac;
using Common.Log;
using Lykke.Messages.Email;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.GoogleAnalyticsWrapper.Client;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.Registration;
using Lykke.Service.Session.Client;
using Lykke.SettingsReader;
using WebAuth.Settings;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.IpGeoLocation;

namespace WebAuth.Modules
{
    public class ClientServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public ClientServiceModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterClientSessionClient(_settings.CurrentValue.OAuth.SessionApiUrl, _log);
            builder.RegisterRegistrationClient(_settings.CurrentValue.OAuth.RegistrationApiUrl, _log);
            builder.RegisterInstance<IPersonalDataService>(
                    new PersonalDataService(_settings.CurrentValue.PersonalDataServiceSettings, _log))
                .SingleInstance();

            builder.RegisterEmailSenderViaAzureQueueMessageProducer(_settings.ConnectionString(x => x.OAuth.Db.ClientPersonalInfoConnString));
            builder.RegisterLykkeServiceClient(_settings.CurrentValue.ClientAccountServiceClient.ServiceUrl);
            builder.RegisterIpGeoLocationClient(_settings.CurrentValue.IpGeoLocationServiceClient.ServiceUrl, _log);

            builder.RegisterConfirmationCodesClient(_settings.CurrentValue.ConfirmationCodesClient);
            builder.RegisterGoogleAnalyticsWrapperClient(_settings.CurrentValue.GaWrapperServiceClient.ServiceUrl);
        }
    }
}
