using Autofac;
using Common.Log;
using Core.Settings;
using Lykke.Messages.Email;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Settings;
using Lykke.Service.Registration;
using Lykke.SettingsReader;

namespace WebAuth.Modules
{
    public class ClientServiceModule : Module
    {
        private readonly IReloadingManager<OAuthSettings> _settings;
        private readonly ILog _log;

        public ClientServiceModule(IReloadingManager<OAuthSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterRegistrationClient(_settings.CurrentValue.OAuth.RegistrationApiUrl, _log);
            builder.RegisterInstance<IPersonalDataService>(
                    new PersonalDataService(new PersonalDataServiceSettings
                    {
                        ApiKey = _settings.CurrentValue.PersonalDataServiceSettings.ApiKey,
                        ServiceUri = _settings.CurrentValue.PersonalDataServiceSettings.ServiceUri
                    }, _log))
                .SingleInstance();

            builder.RegisterEmailSenderViaAzureQueueMessageProducer(_settings.ConnectionString(x => x.OAuth.Db.ClientPersonalInfoConnString));
        }
    }
}
