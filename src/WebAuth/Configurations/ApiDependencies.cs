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
        public static BaseSettings Settings;

        public static IServiceProvider Create(IServiceCollection services, BaseSettings baseSettings)
        {
            var container = new Container();

            container.Configure(
                _ =>
                {
                    _.AddRegistry(new AzureDataAccessConfig(baseSettings));
                    _.AddRegistry(new BusinessServiceConfig(baseSettings));
                });

            container.Populate(services);

            return container.GetInstance<IServiceProvider>();
        }

    }
}