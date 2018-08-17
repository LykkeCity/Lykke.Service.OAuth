﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using AzureStorage.Blob;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Core.Extensions;
using Lykke.AzureQueueIntegration;
using Lykke.Logs;
using Lykke.Logs.Slack;
using Lykke.SettingsReader;
using Lykke.SettingsReader.ReloadingManager;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using WebAuth.EventFilter;
using WebAuth.Providers;
using WebAuth.Modules;
using WebAuth.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using WebAuth.Settings.ServiceSettings;
using LogLevel = Lykke.Logs.LogLevel;

namespace WebAuth
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; set; }
        public ILog Log { get; private set; }
        private AppSettings _settings;
        private const string BlobSource = "blob:";
        private const string DataSource = "data:";
        private const string AnySource = "*";
        private const string DataProtectionContainerName = "data-protection-container-name";

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                var settings = Configuration.LoadSettings<AppSettings>();
                _settings = settings.CurrentValue;

                var certBlob = AzureBlobStorage.Create(ConstantReloadingManager.From(_settings.OAuth.Db.CertStorageConnectionString));
 
                var cert = certBlob.GetAsync(Certificates.ContainerName, _settings.OAuth.Certificates.OpenIdConnectCertName).Result.ToBytes();

                var xcert = new X509Certificate2(cert, _settings.OAuth.Certificates.OpenIdConnectCertPassword);

                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = OpenIdConnectConstantsExt.Auth.DefaultScheme;
                })
                .AddCookie(OpenIdConnectConstantsExt.Auth.DefaultScheme, options =>
                {
                    options.Cookie.Name = CookieAuthenticationDefaults.CookiePrefix + OpenIdConnectConstantsExt.Auth.DefaultScheme;
                    options.ExpireTimeSpan = TimeSpan.FromHours(24);
                    options.LoginPath = new PathString("/signin");
                    options.LogoutPath = new PathString("/signout");
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = _settings.OAuth.CookieSettings.SameSiteMode;
                })
                .AddOAuthValidation()
                .AddOpenIdConnectServer(options =>
                {
                    options.ProviderType = typeof(AuthorizationProvider);
                    options.AuthorizationEndpointPath = "/connect/authorize";
                    options.LogoutEndpointPath = "/connect/logout";
                    options.TokenEndpointPath = "/connect/token";
                    options.UserinfoEndpointPath = "/connect/default_userinfo";
                    options.ApplicationCanDisplayErrors = true;
                    options.AllowInsecureHttp = Environment.IsDevelopment();
                    options.SigningCredentials.AddCertificate(xcert);
                });

                services.AddLocalization(options => options.ResourcesPath = "Resources");

                services.AddCors(options =>
                {
                    options.AddPolicy("Lykke", policy =>
                    {
                        policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
                });

                services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization()
                    .AddMvcOptions(o => { o.Filters.Add(typeof(UnhandledExceptionFilter)); });

                services.AddAutoMapper();

                services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });

                var builder = new ContainerBuilder();

                Configuration.CheckDependenciesAsync(settings, _settings.SlackNotifications.AzureQueue.ConnectionString, _settings.SlackNotifications.AzureQueue.QueueName,
                    $"{PlatformServices.Default.Application.ApplicationName} {PlatformServices.Default.Application.ApplicationVersion}");

                services.AddDataProtection()
                    // Do not change this value. Otherwise the key will be invalid.
                    .SetApplicationName("Lykke.Service.OAuth")
                    .PersistKeysToAzureBlobStorage(SetupDataProtectionStorage(_settings.OAuth.Db.DataProtectionConnString), $"{DataProtectionContainerName}/cookie-keys/keys.xml");

                Log = CreateLogWithSlack(services, settings);

                builder.RegisterInstance(Log).As<ILog>().SingleInstance();

                builder.RegisterModule(new WebModule(settings));
                builder.RegisterModule(new DbModule(settings, Log));
                builder.RegisterModule(new BusinessModule(settings, Log));
                builder.RegisterModule(new ClientServiceModule(settings, Log));

                builder.Populate(services);
                ApplicationContainer = builder.Build();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex);
                throw;
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                }

                var forwardedHeaderOptions = new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedProto
                };

                app.UseForwardedHeaders(forwardedHeaderOptions);
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("en-AU"),
                    new CultureInfo("en-GB"),
                    new CultureInfo("en"),
                    new CultureInfo("ru-RU"),
                    new CultureInfo("ru"),
                    new CultureInfo("fr-FR"),
                    new CultureInfo("fr")
                };

                app.UseRequestLocalization(new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture("en-GB"),
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures
                });

                app.UseCors("Lykke");

                app.UseAuthentication();

                app.UseSession();

                app.UseCsp(options => options.DefaultSources(directive => directive.Self().CustomSources(BlobSource, "www.google.com"))
                    .ImageSources(directive => directive.Self()
                        .CustomSources(AnySource, DataSource, BlobSource))
                    .ScriptSources(directive =>
                    {
                        directive.Self().UnsafeInline();

                        if (_settings.OAuth.Csp.ScriptSources.Any())
                            directive.CustomSources(_settings.OAuth.Csp.ScriptSources);
                    })
                    .StyleSources(directive =>
                    {
                        directive.Self().UnsafeInline();

                        if (_settings.OAuth.Csp.StyleSources.Any())
                            directive.CustomSources(_settings.OAuth.Csp.StyleSources);
                    })
                    .FontSources(x =>
                    {
                        x.SelfSrc = true;

                        if (_settings.OAuth.Csp.FontSources.Any())
                            x.CustomSources = _settings.OAuth.Csp.FontSources;
                    }));

                app.UseXContentTypeOptions();

                app.UseXfo(options => options.Deny());

                app.UseXXssProtection(options => options.EnabledWithBlockMode());

                app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

                app.UseStaticFiles();

                app.UseMvc();

                appLifetime.ApplicationStarted.Register(() => StartApplication().Wait());
                appLifetime.ApplicationStopping.Register(() => StopApplication().Wait());
                appLifetime.ApplicationStopped.Register(() => CleanUp().Wait());
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet recieve and process requests here
            }
            catch (Exception ex)
            {
                await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                // NOTE: Service still can recieve and process requests here, so take care about it if you add logic here.
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), "", ex);
                }
                throw;
            }
        }

        private async Task CleanUp()
        {
            try
            {
                // NOTE: Service can't recieve and process requests here, so you can destroy all resources
                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (Log as IDisposable)?.Dispose();
                }
                throw;
            }
        }


        private static CloudStorageAccount SetupDataProtectionStorage(string dbDataProtectionConnString)
        {

            var storageAccount = CloudStorageAccount.Parse(dbDataProtectionConnString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(DataProtectionContainerName);

            container.CreateIfNotExistsAsync(new BlobRequestOptions { RetryPolicy = new ExponentialRetry() }, new OperationContext()).GetAwaiter().GetResult();

            return storageAccount;
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.OAuth.Db.LogsConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            if (string.IsNullOrEmpty(dbLogConnectionString))
            {
                consoleLogger.WriteWarningAsync(nameof(Startup), nameof(CreateLogWithSlack), "Table loggger is not inited").Wait();
                return aggregateLogger;
            }

            if (dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}"))
                throw new InvalidOperationException($"LogsConnString {dbLogConnectionString} is not filled in settings");

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "OauthLog", consoleLogger),
                consoleLogger);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

            // Creating azure storage logger, which logs own messages to concole log
            var azureStorageLogger = new LykkeLogToAzureStorage(
                persistenceManager,
                slackNotificationsManager,
                consoleLogger);

            azureStorageLogger.Start();

            aggregateLogger.AddLog(azureStorageLogger);

            var logToSlack = LykkeLogToSlack.Create(slackService, "oauth", LogLevel.Error | LogLevel.FatalError | LogLevel.Warning);
            aggregateLogger.AddLog(logToSlack);

            return aggregateLogger;
        }
    }
}
