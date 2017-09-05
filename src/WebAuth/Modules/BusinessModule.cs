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

namespace WebAuth.Modules
{
    public class BusinessModule : Module
    {
        private readonly OAuthSettings _settings;
        private readonly ILog _log;
        private IApplicationService _x;
        public BusinessModule(OAuthSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JobGeolocationDataUpdater>()
                .As<IRegistrationConsumer>()
                .SingleInstance();

            builder.RegisterType<CountryService>()
                .As<ICountryService>()
                .SingleInstance();

            builder.RegisterType<IpGeoLocationService>()
                .As<IIpGeoLocationService>()
                .SingleInstance();

            builder.RegisterType<JobGeolocationDataUpdater>()
                .As<IRegistrationConsumer>()
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
