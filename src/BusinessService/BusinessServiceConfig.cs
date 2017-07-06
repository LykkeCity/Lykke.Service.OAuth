using BusinessService.Clients;
using BusinessService.Country;
using BusinessService.Infrastructure;
using BusinessService.Messages.Email;
using BusinessService.Messages.Settings;
using Core.Clients;
using Core.Country;
using Core.Messages.Email;
using Core.Settings;
using StructureMap;

namespace BusinessService
{
    public class BusinessServiceConfig : Registry
    {
        public BusinessServiceConfig(IOAuthSettings settings)
        {
            Scan(_ =>
            {
                // Declare which assemblies to scan
                _.TheCallingAssembly();

                _.AddAllTypesOf<IApplicationService>();
                _.WithDefaultConventions();
            });

            var serviceBusSettings = new ServiceBusEmailSettings
            {
                Key = settings.EmailServiceBus.Key,
                QueueName = settings.EmailServiceBus.QueueName,
                NamespaceUrl = settings.EmailServiceBus.NamespaceUrl,
                PolicyName = settings.EmailServiceBus.PolicyName
            };

            For<ServiceBusEmailSettings>().Use(serviceBusSettings).Singleton();
            For<IEmailSender>().Use<ServiceBusEmailSender>();

            For<IRegistrationConsumer>().Use<JobGeolocationDataUpdater>();
            For<ICountryService>().Use<CountryService>().Ctor<IOAuthSettings>().Is(settings);
            For<IIpGeoLocationService>().Use<IpGeoLocationService>().Ctor<IOAuthSettings>().Is(settings);

            For<IPersonalDataService>().Use<PersonalDataService>().Ctor<PersonalDataServiceSettings>().Is(new PersonalDataServiceSettings
            {
                ServiceUri = settings.OAuth.PersonalDataServiceUrl,
                ApiKey = settings.OAuth.PersonalDataApiKey
            });
        }
    }
}