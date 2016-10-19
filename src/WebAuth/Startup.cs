using System;
using System.Globalization;
using System.IO;
using AspNet.Security.OAuth.Validation;
using AutoMapper;
using AzureDataAccess.Settings;
using Core.Application;
using Core.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NWebsec.AspNetCore.Middleware;
using WebAuth.Configurations;
using WebAuth.Providers;

namespace WebAuth
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public virtual IConfigurationRoot Configuration { get; }

        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var baseSettings = ReadGeneralSettings();
            return ConfigureWebServices(services, baseSettings);
        }

        public IServiceProvider ConfigureWebServices(IServiceCollection services, BaseSettings settings)
        {
            services.AddAuthentication(options => { options.SignInScheme = "ServerCookie"; });

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc().AddViewLocalization().AddDataAnnotationsLocalization();

            services.AddDistributedMemoryCache();

            services.AddAutoMapper();

            services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });

            WebDependencies.Create(services);

            return ApiDependencies.Create(services, settings);
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

                app.Use(async (context, next) =>
                {
                    if (context.Request.IsHttps)
                    {
                        await next();
                    }
                    else
                    {
                        var withHttps =
                            $"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
                        context.Response.Redirect(withHttps);
                    }
                });
            }

            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("en-AU"),
                new CultureInfo("en-GB"),
                new CultureInfo("en"),
                new CultureInfo("ru-RU"),
                new CultureInfo("ru"),
                new CultureInfo("fr-FR"),
                new CultureInfo("fr"),
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-GB"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

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
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
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

                if (env.IsDevelopment())
                    options.AllowInsecureHttp = true;
            });

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

        private static BaseSettings ReadGeneralSettings()
        {
            var settingsData = ReadSettingsFile();

            if (string.IsNullOrWhiteSpace(settingsData))
            {
                Console.WriteLine("Please, provide generalsettings.json file");
                return null;
            }

            var settings = GeneralSettingsReader.ReadSettingsFromData<BaseSettings>(settingsData);

            CheckSettings(settings);

            return settings;
        }

        private static void CheckSettings(BaseSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Db?.ClientPersonalInfoConnString))
                throw new Exception("ClientPersonalInfoConnString is missing");
            if (string.IsNullOrWhiteSpace(settings.Db?.LogsConnString))
                throw new Exception("LogsConnString is missing");
            if (string.IsNullOrWhiteSpace(settings.LykkeServiceApi?.ServiceUri))
                throw new Exception("ServiceUri is missing");
        }

        private static string ReadSettingsFile()
        {
            try
            {
#if DEBUG
                return File.ReadAllText(@"..\..\..\settings\generalsettings.json");
#else
                return File.ReadAllText("generalsettings.json");
#endif
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}