using Autofac;
using BusinessService;
using BusinessService.Email;
using Common.Log;
using Core.Email;
using Core.Recaptcha;
using Lykke.SettingsReader;
using WebAuth.Settings;

namespace WebAuth.Modules
{
    public class BusinessModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;
        
        public BusinessModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EmailFacadeService>().As<IEmailFacadeService>();
            builder.RegisterType<RecaptchaService>()
                .As<IRecaptchaService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.OAuth.Security.RecaptchaSecrect))
                .SingleInstance();
        }
    }
}
