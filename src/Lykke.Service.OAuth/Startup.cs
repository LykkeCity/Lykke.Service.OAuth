using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using AzureStorage.Blob;
using Common;
using Common.Log;
using Core.Extensions;
using Core.Services;
using IdentityServer4.AccessTokenValidation;
using Lykke.Common.ApiLibrary.Authentication;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Logs;
using Lykke.Service.OAuth.Extensions;
using Lykke.Service.OAuth.Modules;
using Lykke.SettingsReader;
using Lykke.SettingsReader.ReloadingManager;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using WebAuth.Modules;
using WebAuth.Providers;
using WebAuth.Settings;
using WebAuth.Settings.ServiceSettings;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Lykke.Service.OAuth.Extensions.PasswordValidation;
using Lykke.Service.OAuth.Middleware;
using LykkeApiErrorMiddleware = Lykke.Service.OAuth.Middleware.LykkeApiErrorMiddleware;

namespace WebAuth
{
    public class Startup
    {
        private IConfigurationRoot Configuration { get; }
        private IHostingEnvironment Environment { get; }
        private IContainer ApplicationContainer { get; set; }
        private ILog Log { get; set; }
        private AppSettings _settings;
        private const string BlobSource = "blob:";
        private const string DataSource = "data:";
        private const string AnySource = "*";
        private const string DataProtectionContainerName = "data-protection-container-name";
        private IHealthNotifier HealthNotifier { get; set; }

        private static CultureInfo[] _supportedCultures =
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
                var settings = Configuration.LoadSettings<AppSettings>(options =>
                {
                    options.SetConnString(x => x.SlackNotifications.AzureQueue.ConnectionString);
                    options.SetQueueName(x => x.SlackNotifications.AzureQueue.QueueName);
                    options.SenderName = "OAuth service";
                });

                services.AddLykkeLogging(settings.Nested(x => x.OAuth.Db.LogsConnString),
                    "OauthLog",
                    settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                    settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
                , options => { options.AddFilter("AspNet.Security.OpenIdConnect.Server", LogLevel.Error); });

                _settings = settings.CurrentValue;

                var certBlob =
                    AzureBlobStorage.Create(
                        ConstantReloadingManager.From(_settings.OAuth.Db.CertStorageConnectionString));

                var cert = certBlob
                    .GetAsync(Certificates.ContainerName, _settings.OAuth.Certificates.OpenIdConnectCertName).Result
                    .ToBytes();

                var xcert = new X509Certificate2(cert, _settings.OAuth.Certificates.OpenIdConnectCertPassword);

                services.AddDiscoveryCache(_settings.OAuth.ExternalProvidersSettings.IroncladAuth.Authority);

                services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = OpenIdConnectConstantsExt.Auth.DefaultScheme;
                    })

                    .AddCookie(OpenIdConnectConstantsExt.Auth.DefaultScheme, options =>
                    {
                        options.Cookie.Name = CookieAuthenticationDefaults.CookiePrefix +
                                              OpenIdConnectConstantsExt.Auth.DefaultScheme;
                        // Lifetime of AuthenticationTicket for silent refresh. 
                        options.ExpireTimeSpan = TimeSpan.FromDays(60);
                        options.LoginPath = new PathString("/signin");
                        options.LogoutPath = new PathString("/signout");
                        options.Cookie.HttpOnly = true;
                        options.Cookie.SameSite = _settings.OAuth.CookieSettings.SameSiteMode;
                        options.EventsType = typeof(CustomCookieAuthenticationEvents);
                    })

                    // This cookie is used for external provider authentication.
                    .AddCookie(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme, options =>
                    {
                        options.Cookie.HttpOnly = true;
                        options.Cookie.SameSite = _settings.OAuth.CookieSettings.SameSiteMode;
                    })

                    .AddIronclad(_settings.OAuth.ExternalProvidersSettings.IroncladAuth)

                    .AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme,
                        options =>
                        {
                            var config = settings.Nested(n => n.OAuth.ResourceServerSettings).CurrentValue;
                            options.Authority = config.Authority;
                            options.ApiName = config.ClientId;
                            options.ApiSecret = config.ClientSecret;
                        })
                    .AddLykkeAuthentication(OpenIdConnectConstantsExt.Auth.LykkeScheme, options =>
                    {
                        options.Authority = _settings.OAuth.ResourceServerSettings.Authority;
                        options.ClientId = _settings.OAuth.ResourceServerSettings.ClientId;
                        options.ClientSecret = _settings.OAuth.ResourceServerSettings.ClientSecret;
                        options.NameClaimType = _settings.OAuth.ResourceServerSettings.NameClaimType;
                        options.EnableCaching = _settings.OAuth.ResourceServerSettings.EnableCaching;
                        options.CacheDuration = _settings.OAuth.ResourceServerSettings.CacheDuration;
                        options.SkipTokensWithDots = _settings.OAuth.ResourceServerSettings.SkipTokensWithDots;
                    })
                    .AddOpenIdConnectServer(options =>
                    {
                        options.ProviderType = typeof(AuthorizationProvider);
                        options.AuthorizationEndpointPath = "/connect/authorize";
                        options.LogoutEndpointPath = "/connect/logout";
                        options.TokenEndpointPath = "/connect/token";
                        options.IntrospectionEndpointPath = "/connect/introspection";
                        options.UserinfoEndpointPath = "/connect/default_userinfo";
                        options.ApplicationCanDisplayErrors = true;
                        options.AllowInsecureHttp = Environment.IsDevelopment();
                        options.SigningCredentials.AddCertificate(xcert);
                        options.AccessTokenLifetime = TimeSpan.FromMinutes(10);
                        options.RefreshTokenLifetime = TimeSpan.FromDays(30);
                        options.UseSlidingExpiration = true;
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
                    .AddDataAnnotationsLocalization();

                services.AddAutoMapper();

                services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });

                var builder = new ContainerBuilder();

                services.AddDataProtection()
                    // Do not change this value. Otherwise the key will be invalid.
                    .SetApplicationName("Lykke.Service.OAuth")
                    .PersistKeysToAzureBlobStorage(
                        SetupDataProtectionStorage(_settings.OAuth.Db.DataProtectionConnString),
                        $"{DataProtectionContainerName}/cookie-keys/keys.xml");
                
                services.AddPwnedPasswordHttpClient();

                services.AddSwaggerGen(opt =>
                {
                    opt.DefaultLykkeConfiguration("v1", "Lykke OAuth Server");
                });

                builder.RegisterModule(new WebModule(settings));
                builder.RegisterModule(new DbModule(settings));
                builder.RegisterModule(new BusinessModule(settings));
                builder.RegisterModule(new ClientServiceModule(settings));
                builder.RegisterModule(new ServiceModule(settings));
                builder.RegisterModule(new ExceptionsModule());
                builder.RegisterModule(new CqrsModule(settings));

                builder.Populate(services);
                ApplicationContainer = builder.Build();

                Log = ApplicationContainer.Resolve<ILogFactory>().CreateLog(this);
                HealthNotifier = ApplicationContainer.Resolve<IHealthNotifier>();
                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            try
            {
                app.UseLykkeMiddleware(ex => new { message = "Technical problem" });

                app.UseMiddleware<LykkeApiErrorMiddleware>();

                app.UseMiddleware<RedirectResponseOverride>();

                app.UseLykkeForwardedHeaders();

                app.UseRequestLocalization(new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture("en-GB"),
                    SupportedCultures = _supportedCultures,
                    SupportedUICultures = _supportedCultures
                });

                app.UseCors("Lykke");

                app.UseAuthentication();

                app.UseSession();

                app.UseCsp(options => options
                    .DefaultSources(directive => directive.Self().CustomSources(BlobSource, "www.google.com"))
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

                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });

                appLifetime.ApplicationStarted.Register(StartApplication);
                appLifetime.ApplicationStopped.Register(CleanUp);
            }
            catch (Exception ex)
            {
                Log.Critical(ex);
                throw;
            }
        }

        private void StartApplication()
        {
            try
            {
                ApplicationContainer.Resolve<IStartupManager>().Start();

                ApplicationContainer.Resolve<ICqrsEngine>().Start();

                HealthNotifier.Notify($"Env: {Program.EnvInfo}", "Started");
            }
            catch (Exception ex)
            {
                Log.Critical(ex);
                throw;
            }
        }

        private void CleanUp()
        {
            try
            {
                // NOTE: Service can't receive and process requests here, so you can destroy all resources

                HealthNotifier?.Notify($"Env: {Program.EnvInfo}", "Terminating");

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        private static CloudStorageAccount SetupDataProtectionStorage(string dbDataProtectionConnString)
        {
            var storageAccount = CloudStorageAccount.Parse(dbDataProtectionConnString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(DataProtectionContainerName);

            container.CreateIfNotExistsAsync(new BlobRequestOptions {RetryPolicy = new ExponentialRetry()},
                new OperationContext()).GetAwaiter().GetResult();

            return storageAccount;
        }
    }
}
