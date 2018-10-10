using System.Collections.Generic;
using Autofac;
using Core.ExternalProvider;
using Core.Services;
using Lykke.Service.OAuth.Services;
using Lykke.Service.OAuth.Services.ExternalProvider;
using Lykke.SettingsReader;
using WebAuth.Settings;

namespace Lykke.Service.OAuth.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TokenService>().As<ITokenService>().SingleInstance();
            builder.RegisterType<ValidationService>().As<IValidationService>().SingleInstance();
            builder.RegisterType<ExternalUserService>().As<IExternalUserService>().SingleInstance();

            builder.RegisterType<ExternalProviderService>()
                .WithParameter(
                    (info, context) => info.ParameterType == typeof(IEnumerable<ExternalIdentityProvider>),
                    (info, context) =>
                    {
                        return _settings.Nested(settings =>
                            settings.OAuth.ExternalProvidersSettings.ExternalIdentityProviders).CurrentValue;
                    })
                .As<IExternalProviderService>().SingleInstance();


            builder.Register(context =>
                {
                    return new LifetimeSettingsProvider(_settings.Nested(settings => settings.OAuth.LifetimeSettings)
                        .CurrentValue);
                })
                .As<ILifetimeSettingsProvider>()
                .SingleInstance();
        }
    }
}
