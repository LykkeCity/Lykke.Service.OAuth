using System.Collections.Generic;
using Autofac;
using Core.Countries;
using Core.ExternalProvider;
using Core.PasswordValidation;
using Core.Services;
using Lykke.Common;
using Lykke.Service.OAuth.Services;
using Lykke.Service.OAuth.Services.Countries;
using Lykke.Service.OAuth.Services.ExternalProvider;
using Lykke.Service.OAuth.Services.PasswordValidation;
using Lykke.Service.OAuth.Services.PasswordValidation.Validators;
using Lykke.SettingsReader;
using WebAuth.Settings;

namespace Lykke.Service.OAuth.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        private readonly int _bCryptWorkFactor;

        private readonly IEnumerable<string> _restrictedCountriesOfResidenceIso2;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
            _bCryptWorkFactor = _settings.CurrentValue.OAuth.Security.BCryptWorkFactor;
            _restrictedCountriesOfResidenceIso2 = _settings.CurrentValue.OAuth.RegistrationProcessSettings.RestrictedCountriesOfResidenceIso2;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TokenService>().As<ITokenService>().SingleInstance();

            builder.RegisterType<ValidationService>().As<IValidationService>().SingleInstance();

            builder.RegisterType<ExternalProviderService>().As<IExternalProviderService>().SingleInstance();

            builder.RegisterType<ExternalUserService>().As<IExternalUserService>().SingleInstance();

            builder.RegisterType<CountriesService>()
                .WithParameter(TypedParameter.From(new CountryPhoneCodes().GetCountries()))
                .WithParameter(TypedParameter.From(_restrictedCountriesOfResidenceIso2))
                .As<ICountriesService>().SingleInstance();

            builder.RegisterType<EmailValidationService>()
                .As<IEmailValidationService>();

            builder.RegisterType<BCryptService>()
                .WithParameter(TypedParameter.From(_bCryptWorkFactor))
                .As<IBCryptService>();

            builder.RegisterType<StartupManager>()
                .WithParameter(TypedParameter.From(_bCryptWorkFactor))
                .WithParameter(TypedParameter.From(_restrictedCountriesOfResidenceIso2))
                .As<IStartupManager>()
                .SingleInstance();

            #region PasswordValidators
            builder.RegisterType<PwnedPasswordsValidator>().As<IPasswordValidator>().SingleInstance();
            builder.RegisterType<PasswordValidationService>().As<IPasswordValidationService>().SingleInstance();
            #endregion
        }
    }
}
