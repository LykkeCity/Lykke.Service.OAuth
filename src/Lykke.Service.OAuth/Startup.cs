using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using AzureStorage.Blob;
using Common;
using Common.Log;
using Core.Extensions;
using IdentityModel;
using IdentityServer4.AccessTokenValidation;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.Service.OAuth.Events;
using Lykke.Service.OAuth.Modules;
using Lykke.SettingsReader;
using Lykke.SettingsReader.ReloadingManager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using WebAuth.EventFilter;
using WebAuth.Modules;
using WebAuth.Providers;
using WebAuth.Settings;
using WebAuth.Settings.ServiceSettings;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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

                services.AddAuthorization(options =>
                {
                    // Policy that is allowed only for users signed in through Lykke.
                    options.AddPolicy(OpenIdConnectConstantsExt.Policies.OnlyLykkeSignIn, policyBuilder =>
                    {
                        policyBuilder.AddAuthenticationSchemes(OpenIdConnectConstantsExt.Auth.DefaultScheme, IdentityServerAuthenticationDefaults.AuthenticationScheme);
                        policyBuilder.AddRequirements(new ClaimsAuthorizationRequirement(OpenIdConnectConstantsExt.Claims.SignInProvider, new List<string>{OpenIdConnectConstantsExt.Providers.Lykke}));
                    });
                });

                services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = OpenIdConnectConstantsExt.Auth.DefaultScheme;
                    })

                    .AddCookie(OpenIdConnectConstantsExt.Auth.DefaultScheme, options =>
                    {
                        options.Cookie.Name = CookieAuthenticationDefaults.CookiePrefix +
                                              OpenIdConnectConstantsExt.Auth.DefaultScheme;
                        // Lifetime of AuthenticationTicket for silent refresh. 
                        options.ExpireTimeSpan = _settings.OAuth.LifetimeSettings.SilentRefreshCookieLifetime;
                        options.LoginPath = new PathString("/signin");
                        options.LogoutPath = new PathString("/signout");
                        options.Cookie.HttpOnly = true;
                        options.Cookie.SameSite = _settings.OAuth.CookieSettings.SameSiteMode;
                        options.EventsType = typeof(CustomCookieAuthenticationEvents);
                    })
                    .AddCookie(OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme, options =>
                    {
                        options.Cookie.HttpOnly = true;
                        options.Cookie.SameSite = _settings.OAuth.CookieSettings.SameSiteMode;
                    })

                    .AddOpenIdConnect(OpenIdConnectConstantsExt.Auth.VmoolaAuthenticationScheme, OpenIdConnectConstantsExt.Providers.vMoola, options =>
                    {
                        options.SignInScheme = OpenIdConnectConstantsExt.Auth.ExternalAuthenticationScheme;

                        var vmoolaSettings =
                                _settings.OAuth.ExternalProvidersSettings.ExternalIdentityProviders.FirstOrDefault(
                                    provider => provider.Id == OpenIdConnectConstantsExt.Providers.vMoola);
                        if (vmoolaSettings == null)
                        {
                            throw new InvalidOperationException($"Configuration not found for provider: {OpenIdConnectConstantsExt.Providers.vMoola}");
                        }
                        options.Authority = vmoolaSettings.Authority;
                        options.ClientId = vmoolaSettings.ClientId;
                        options.ClientSecret = vmoolaSettings.ClientSecret;
                        options.ResponseType = vmoolaSettings.ResponseType;
                        options.TokenValidationParameters.ValidIssuers = new List<string>(vmoolaSettings.ValidIssuers);
                        
                        options.SaveTokens = true;
                        options.DisableTelemetry = true;

                        // Allow issuer and audience to be available as claims.
                        options.ClaimActions.Remove(JwtClaimTypes.Issuer);
                        options.ClaimActions.Remove(JwtClaimTypes.Audience);

                        foreach (var map in vmoolaSettings.ClaimsMapping)
                        {
                            var from = map.Key;
                            var to = map.Value;

                            options.ClaimActions.Remove(from);
                            options.ClaimActions.Remove(to);
                            options.ClaimActions.MapUniqueJsonKey(to, from);
                        }

                        //Get claims from user info to map them.
                        options.GetClaimsFromUserInfoEndpoint = true;
                        options.EventsType = typeof(ExternalOpenIdConnectEvents);
                    })

                    .AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme,
                        options =>
                        {

                            var config = settings.Nested(n => n.OAuth.ResourceServerSettings).CurrentValue;
                            options.Authority = config.Authority;
                            options.ApiName = config.ClientId;
                            options.ApiSecret = config.ClientSecret;
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
                        options.AccessTokenLifetime = _settings.OAuth.LifetimeSettings.AccessTokenLifetime;
                        options.RefreshTokenLifetime = _settings.OAuth.LifetimeSettings.RefreshTokenLifetime;
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
                    .AddDataAnnotationsLocalization()
                    .AddMvcOptions(o => { o.Filters.Add(typeof(UnhandledExceptionFilter)); });

                services.AddAutoMapper();

                services.AddSession(options => { options.IdleTimeout = _settings.OAuth.LifetimeSettings.SessionIdleTimeout; });

                var builder = new ContainerBuilder();

                services.AddDataProtection()
                    // Do not change this value. Otherwise the key will be invalid.
                    .SetApplicationName("Lykke.Service.OAuth")
                    .PersistKeysToAzureBlobStorage(
                        SetupDataProtectionStorage(_settings.OAuth.Db.DataProtectionConnString),
                        $"{DataProtectionContainerName}/cookie-keys/keys.xml");

                builder.RegisterModule(new WebModule(settings));
                builder.RegisterModule(new DbModule(settings));
                builder.RegisterModule(new BusinessModule(settings));
                builder.RegisterModule(new ClientServiceModule(settings));
                builder.RegisterModule(new ServiceModule(settings));

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
                if (env.IsDevelopment())
                    app.UseDeveloperExceptionPage();
                else
                    app.UseExceptionHandler("/Home/Error");

                app.UseLykkeForwardedHeaders();
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
