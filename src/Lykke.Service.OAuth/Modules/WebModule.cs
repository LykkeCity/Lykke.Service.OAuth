using Autofac;
using Lykke.Service.OAuth.Factories;
using Lykke.Service.OAuth.Managers;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using WebAuth.ActionHandlers;
using WebAuth.Managers;
using WebAuth.Providers;
using WebAuth.Settings;

namespace WebAuth.Modules
{
    public class WebModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public WebModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UserManager>().As<IUserManager>().SingleInstance();
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().SingleInstance();
            builder.RegisterType<UrlHelperFactory>().As<IUrlHelperFactory>().SingleInstance();
            builder.RegisterType<ProfileActionHandler>().AsSelf().SingleInstance();
            builder.RegisterType<AuthorizationProvider>().AsSelf().SingleInstance();
            builder.RegisterType<CustomCookieAuthenticationEvents>().SingleInstance();
            builder.RegisterType<RequestModelFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(_settings.CurrentValue.OAuth.Security);
            builder.RegisterInstance(_settings.CurrentValue.OAuth.FeatureToggle);
        }
    }
}
