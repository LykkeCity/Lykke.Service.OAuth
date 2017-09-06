using System;
using System.Globalization;
using AspNet.Security.OAuth.Validation;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using AzureDataAccess;
using Common.Log;
using Core.Application;
using Core.Settings;
using Lykke.Logs;
using Lykke.Service.Registration;
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
            if (Environment.IsProduction() && string.IsNullOrEmpty(Configuration["SettingsUrl"]))
            {
                throw new Exception("SettingsUrl is not found");
            }

            OAuthSettings settings = Environment.IsDevelopment()
                ? Configuration.Get<OAuthSettings>()
                : HttpSettingsLoader.Load<OAuthSettings>(Configuration.GetValue<string>("SettingsUrl"));

            services.AddSingleton<IOAuthSettings>(settings);

            var applicationRepository = AzureRepoFactories.Applications.CreateApplicationsRepository(settings.OAuth.Db.ClientPersonalInfoConnString, null);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.Cookie.Name = CookieAuthenticationDefaults.CookiePrefix + CookieAuthenticationDefaults.AuthenticationScheme;
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    o.LoginPath = new PathString("/signin");
                    o.LogoutPath = new PathString("/signout");
                })
                .AddOpenIdConnectServer(options =>
                {
                    options.Provider = new AuthorizationProvider(applicationRepository);
                    options.AuthorizationEndpointPath = "/connect/authorize";
                    options.LogoutEndpointPath = "/connect/logout";
                    options.TokenEndpointPath = "/connect/token";
                    options.UserinfoEndpointPath = "/connect/userinfo";
                    options.ApplicationCanDisplayErrors = true;
                    options.AllowInsecureHttp = false;
                })
                .AddOAuthValidation(OAuthValidationDefaults.AuthenticationScheme);

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

            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new Lykke.AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var log = services.UseLogToAzureStorage(settings.OAuth.Db.LogsConnString, slackService,
                "LogWebAuth", new LogToConsole());

            var builder = new ContainerBuilder();

            builder.RegisterInstance(log).As<ILog>().SingleInstance();
            builder.RegisterInstance(applicationRepository).As<IApplicationRepository>().SingleInstance();

            builder.RegisterModule(new WebModule());
            builder.RegisterModule(new DbModule(settings, log));
            builder.RegisterModule(new BusinessModule(settings, log));

            builder.RegisterRegistrationClient(settings.OAuth.RegistrationApiUrl, log);

            builder.Populate(services);
            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
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

            app.UseAuthentication();

            app.UseSession();

            app.UseCsp(options => options.DefaultSources(directive => directive.Self())
                .ImageSources(directive => directive.Self()
                    .CustomSources("*"))
                .ScriptSources(directive => directive.Self()
                    .UnsafeInline())
                .StyleSources(directive => directive.Self()
                    .UnsafeInline()));

            app.UseXContentTypeOptions();

            app.UseXfo(options => options.Deny());

            app.UseXXssProtection(options => options.EnabledWithBlockMode());

            app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
