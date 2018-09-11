using Autofac;
using AzureDataAccess;
using Common.Log;
using Core.Application;
using Core.Bitcoin;
using Lykke.SettingsReader;
using StackExchange.Redis;
using WebAuth.Settings;
using WebAuth.Settings.ServiceSettings;

namespace WebAuth.Modules
{
    public class DbModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public DbModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var clientPersonalInfoConnString = _settings.ConnectionString(x => x.OAuth.Db.ClientPersonalInfoConnString);

            builder.RegisterInstance(
                AzureRepoFactories.CreateApplicationsRepository(clientPersonalInfoConnString, _log)
            ).As<IApplicationRepository>().SingleInstance();

            builder.RegisterInstance(
                AzureRepoFactories.CreateWalletCredentialsRepository(clientPersonalInfoConnString, _log)
            ).As<IWalletCredentialsRepository>().SingleInstance();

            builder.Register(context =>
                    ConnectionMultiplexer.Connect(_settings.CurrentValue.OAuth.RedisSettings.RedisConfiguration))
                .As<IConnectionMultiplexer>().SingleInstance();
        }
    }
}
