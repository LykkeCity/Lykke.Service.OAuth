using Autofac;
using AzureDataAccess.Application;
using AzureDataAccess.Bitcoin;
using AzureDataAccess.ExternalProvider;
using AzureDataAccess.Registration;
using AzureStorage;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Core.Application;
using Core.Bitcoin;
using Core.Registration;
using Lykke.Common.Log;
using Lykke.SettingsReader;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using WebAuth.Settings;

namespace WebAuth.Modules
{
    internal class DbModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public DbModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var clientPersonalInfoConnString = _settings.ConnectionString(x => x.OAuth.Db.ClientPersonalInfoConnString);

            builder.Register(c =>
                    AzureTableStorage<ApplicationEntity>.Create(clientPersonalInfoConnString, "Applications",
                        c.Resolve<ILogFactory>()))
                .As<INoSQLTableStorage<ApplicationEntity>>()
                .SingleInstance();


            builder.RegisterType<ApplicationRepository>()
                .Named<IApplicationRepository>("notCached");

            builder.RegisterDecorator<IApplicationRepository>(
                (c, inner) => new ApplicationCachedRepository(inner, c.Resolve<IMemoryCache>()), "notCached");


            builder.Register(c => AzureTableStorage<WalletCredentialsEntity>.Create(clientPersonalInfoConnString,
                    "WalletCredentials", c.Resolve<ILogFactory>()))
                .As<INoSQLTableStorage<WalletCredentialsEntity>>()
                .SingleInstance();

            builder.RegisterType<WalletCredentialsRepository>()
                .As<IWalletCredentialsRepository>();

            builder.Register(context =>
            {
                var connectionMultiplexer =
                    ConnectionMultiplexer.Connect(_settings.CurrentValue.OAuth.RedisSettings.RedisConfiguration);
                connectionMultiplexer.IncludeDetailInExceptions = false;
                return connectionMultiplexer;
            }).As<IConnectionMultiplexer>().SingleInstance();


            const string registrationUsersTableName = "RegistrationUser";
            var registrationUserStorageConnString = _settings.ConnectionString(x => x.OAuth.Db.RegistrationUserStorageConnString);
            builder.Register(c => new RegistrationAzureRepository(
                    AzureTableStorage<RegistrationAzureEntity>.Create(
                        registrationUserStorageConnString,
                        registrationUsersTableName, c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(
                        registrationUserStorageConnString,
                        registrationUsersTableName, c.Resolve<ILogFactory>())))
                .As<IRegistrationRepository>()
                .SingleInstance();

            const string ironcladUsersTableName = "IroncladUsers";

            var ironcladUserStorageConnString = _settings.ConnectionString(x => x.OAuth.Db.IroncladUserStorageConnString);
            builder.Register(c => new IroncladUserRepository(
                    AzureTableStorage<IroncladUserEntity>.Create(
                        ironcladUserStorageConnString,
                        ironcladUsersTableName, c.Resolve<ILogFactory>())
                )).As<IRegistrationRepository>()
                .SingleInstance();
        }
    }
}
