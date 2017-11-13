using Autofac;
using BusinessService.Email;
using BusinessService.Kyc;
using Common.Log;
using Core.Email;
using Core.Kyc;
using Core.Settings;
using Lykke.SettingsReader;

namespace WebAuth.Modules
{
    public class BusinessModule : Module
    {
        private readonly IReloadingManager<OAuthSettings> _settings;
        private readonly ILog _log;
        public BusinessModule(IReloadingManager<OAuthSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SrvKycManager>()
                .As<ISrvKycManager>()
                .SingleInstance();

            builder.RegisterType<EmailFacadeService>().As<IEmailFacadeService>();
        }
    }
}
