using Autofac;
using Core.Services;
using Lykke.Service.OAuth.Services;

namespace Lykke.Service.OAuth.Modules
{
    public class ServiceModule : Module
    {
        private readonly int _bCryptWorkFactor;

        public ServiceModule(int bCryptWorkFactor)
        {
            _bCryptWorkFactor = bCryptWorkFactor;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TokenService>().As<ITokenService>().SingleInstance();

            builder.RegisterType<ValidationService>().As<IValidationService>().SingleInstance();

            builder.RegisterType<EmailValidationService>()
                .As<IEmailValidationService>();

            builder.RegisterType<BCryptService>()
                .WithParameter(TypedParameter.From(_bCryptWorkFactor))
                .As<IBCryptService>();

            builder.RegisterType<StartupManager>()
                .WithParameter(TypedParameter.From(_bCryptWorkFactor))
                .As<IStartupManager>()
                .SingleInstance();
        }
    }
}
