using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using AzureStorage.Tables;
using Common.Log;
using Core.Application;
using Core.Settings;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAuth.EventFilter;
using WebAuth.Providers;
using Microsoft.AspNetCore.HttpOverrides;
using WebAuth.Modules;

namespace WebAuth
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; set; }
        public ILog Log { get; private set; }
        private OAuthSettings _settings;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("appsettings.dev.json", true, true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddAuthentication(options => { options.SignInScheme = "ServerCookie"; });

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
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization()
                    .AddMvcOptions(o => { o.Filters.Add(typeof(UnhandledExceptionFilter)); });

                services.AddDistributedMemoryCache();

                services.AddAutoMapper();

                services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });

                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                });

                var builder = new ContainerBuilder();
                var settings = Configuration.LoadSettings<OAuthSettings>();
                _settings = settings.CurrentValue;

                Log = CreateLogWithSlack(services, settings);

                builder.RegisterInstance(Log).As<ILog>().SingleInstance();

                builder.RegisterModule(new WebModule());
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

                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

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

                // Create a new branch where the registered middleware will be executed only for API calls.
                app.UseOAuthValidation(new OAuthValidationOptions

                {
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true
                });

                // Create a new branch where the registered middleware will be executed only for non API calls.
                app.UseCookieAuthentication(new CookieAuthenticationOptions

                {
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true,
                    AuthenticationScheme = "ServerCookie",
                    CookieName = CookieAuthenticationDefaults.CookiePrefix + "ServerCookie",
                    ExpireTimeSpan = TimeSpan.FromHours(24),
                    LoginPath = new PathString("/signin"),
                    LogoutPath = new PathString("/signout")
                });

                app.UseSession();

                var applicationRepository = app.ApplicationServices.GetService<IApplicationRepository>();

                app.UseOpenIdConnectServer(options =>
                {
                    options.Provider = new AuthorizationProvider(applicationRepository);

                // Enable the authorization, logout, token and userinfo endpoints.
                options.AuthorizationEndpointPath = "/connect/authorize";
                    options.LogoutEndpointPath = "/connect/logout";
                    options.TokenEndpointPath = "/connect/token";
                    options.UserinfoEndpointPath = "/connect/userinfo";

                    options.ApplicationCanDisplayErrors = true;
                    options.AllowInsecureHttp = Environment.IsDevelopment();
                });


                app.UseCsp(options => options.DefaultSources(directive => directive.Self())
                    .ImageSources(directive => directive.Self()
                        .CustomSources("*", "data:"))
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

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<OAuthSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new Lykke.AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.OAuth.Db.LogsConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            // Creating azure storage logger, which logs own messages to concole log
            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                    AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "OauthLog", consoleLogger),
                    consoleLogger);

                var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

                var azureStorageLogger = new LykkeLogToAzureStorage(
                    persistenceManager,
                    slackNotificationsManager,
                    consoleLogger);

                azureStorageLogger.Start();

                aggregateLogger.AddLog(azureStorageLogger);
            }

            return aggregateLogger;
        }
    }
}
