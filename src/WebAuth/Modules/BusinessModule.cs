using Autofac;
using BusinessService.Clients;
using BusinessService.Country;
using BusinessService.Infrastructure;
using BusinessService.Kyc;
using Common.Log;
using Core.Clients;
using Core.Country;
using Core.Kyc;
using Core.Settings;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;

namespace WebAuth.Modules
{
    public class BusinessModule : Module
    {
        private readonly OAuthSettings _settings;
        private readonly ILog _log;
        public BusinessModule(OAuthSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CountryService>()
                .As<ICountryService>()
                .SingleInstance();

            builder.RegisterType<SrvKycManager>()
                .As<ISrvKycManager>()
                .SingleInstance();

            builder.RegisterInstance(_settings.PersonalDataServiceSettings).AsSelf().SingleInstance();
            builder.RegisterType<PersonalDataService>()
                .As<IPersonalDataService>()
                .SingleInstance();
        }
    }
}
