using Autofac;
using Core.PasswordValidation;
using Core.Services;
using Lykke.Service.OAuth.Services;
using Lykke.Service.OAuth.Services.PasswordValidation;
using Lykke.Service.OAuth.Services.PasswordValidation.Validators;

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

            #region PasswordValidators
            builder.RegisterType<PwnedPasswordsValidator>().As<IPasswordValidator>().SingleInstance();
            builder.RegisterType<PasswordValidationService>().As<IPasswordValidationService>().SingleInstance();
            #endregion
        }
    }
}
