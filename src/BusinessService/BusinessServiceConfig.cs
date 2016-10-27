using BusinessService.Clients;
using BusinessService.Country;
using BusinessService.Infrastructure;
using Common;
using Core.Clients;
using Core.Country;
using Core.Settings;
using StructureMap;

namespace BusinessService
{
    public class BusinessServiceConfig : Registry
    {
        public BusinessServiceConfig(IBaseSettings settings)
        {
            Scan(_ =>
            {
                // Declare which assemblies to scan
                _.TheCallingAssembly();

                // Built in registration conventions
                _.AddAllTypesOf<IStarter>();
                _.AddAllTypesOf<IApplicationService>();
                _.WithDefaultConventions();
            });

            For<IRegistrationConsumer>().Use<JobGeolocationDataUpdater>();
            For<ICountryService>().Use<CountryService>().Ctor<IBaseSettings>().Is(settings);
            For<IIpGeoLocationService>().Use<IpGeoLocationService>().Ctor<IBaseSettings>().Is(settings);
        }
    }
}