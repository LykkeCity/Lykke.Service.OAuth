using Autofac;
using Common.Log;
using Core.Settings;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Settings;
using Lykke.Service.Registration;

namespace WebAuth.Modules
{
    public class ClientServiceModule : Module
    {
        private readonly OAuthSettings _settings;
        private readonly ILog _log;

        public ClientServiceModule(OAuthSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterRegistrationClient(_settings.OAuth.RegistrationApiUrl, _log);
            builder.RegisterInstance<IPersonalDataService>(
                    new PersonalDataService(new PersonalDataServiceSettings { ApiKey = _settings.PersonalDataServiceSettings.ApiKey, ServiceUri = _settings.PersonalDataServiceSettings.ServiceUri }, _log))
                .SingleInstance();

        }
    }
}
