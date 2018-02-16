using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using WebAuth.ActionHandlers;
using WebAuth.Managers;
using WebAuth.Providers;

namespace WebAuth.Modules
{
    public class WebModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UserManager>().As<IUserManager>().SingleInstance();
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().SingleInstance();
            builder.RegisterType<UrlHelperFactory>().As<IUrlHelperFactory>().SingleInstance();
            builder.RegisterType<ProfileActionHandler>().AsSelf().SingleInstance();
            builder.RegisterType<AuthorizationProvider>().AsSelf().SingleInstance();
        }
    }
}
