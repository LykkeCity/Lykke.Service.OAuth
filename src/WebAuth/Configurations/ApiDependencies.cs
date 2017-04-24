using System;
using AzureDataAccess;
using BusinessService;
using Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace WebAuth.Configurations
{
    public static class ApiDependencies
    {
        public static IServiceProvider Create(IServiceCollection services, IOAuthSettings settings)
        {
            var container = new Container();

            container.Configure(
                _ =>
                {
                    _.AddRegistry(new AzureDataAccessConfig(settings));
                    _.AddRegistry(new BusinessServiceConfig(settings));
                });

            container.Populate(services);

            return container.GetInstance<IServiceProvider>();
        }

    }
}
