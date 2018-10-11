using Autofac;
using Core.Services;
using Lykke.Service.OAuth.Services;

namespace Lykke.Service.OAuth.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TokenService>().As<ITokenService>().SingleInstance();
            builder.RegisterType<ValidationService>().As<IValidationService>().SingleInstance();
        }
    }
}
