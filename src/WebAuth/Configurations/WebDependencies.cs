using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebAuth.ActionHandlers;
using WebAuth.Managers;

namespace WebAuth.Configurations
{
    public class WebDependencies
    {
        public static void Create(IServiceCollection services)
        {
            services.AddSingleton<IUserManager, UserManager>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            services.AddSingleton<AuthenticationActionHandler>();
            services.AddSingleton<ProfileActionHandler>();
            services.AddSingleton<AuthorizationActionHandler>();
        }
    }
}