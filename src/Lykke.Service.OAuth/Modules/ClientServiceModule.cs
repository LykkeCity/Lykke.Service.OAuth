using Autofac;
using Lykke.Common.Log;
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
using Lykke.Service.Kyc;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Client;

namespace WebAuth.Modules
{
    public class ClientServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public ClientServiceModule(
            IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterClientSessionClient(_settings.CurrentValue.SessionServiceClient);
            builder.RegisterRegistrationServiceClient(_settings.CurrentValue.RegistrationServiceClient);
            builder.Register(c => new PersonalDataService(
                    _settings.CurrentValue.PersonalDataServiceSettings,
                    c.Resolve<ILogFactory>()))
                .As<IPersonalDataService>()
                .SingleInstance();

            builder.RegisterEmailSenderViaAzureQueueMessageProducer(_settings.ConnectionString(x => x.OAuth.Db.ClientPersonalInfoConnString));
            builder.RegisterClientAccountClient(_settings.CurrentValue.ClientAccountServiceClient.ServiceUrl);

            builder.Register(c => new IpGeoLocationClient(_settings.CurrentValue.IpGeoLocationServiceClient.ServiceUrl, c.Resolve<ILogFactory>().CreateLog(this)))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterConfirmationCodesClient(_settings.CurrentValue.ConfirmationCodesClient);

            builder.RegisterGoogleAnalyticsWrapperClient(_settings.CurrentValue.GaWrapperServiceClient.ServiceUrl);

            builder.Register(c =>
                new KycDocumentsServiceV2Client(
                    _settings.CurrentValue.KycServiceClient,
                    c.Resolve<ILogFactory>()))
                .As<IKycDocumentsServiceV2>()
                .SingleInstance();
        }
    }
}
