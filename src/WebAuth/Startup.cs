using System;
using System.Globalization;
using AspNet.Security.OAuth.Validation;
using AutoMapper;
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
using WebAuth.EventFilter;
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

        public IConfigurationRoot Configuration { get; }
        private IBaseSettings _settings;

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            _settings = provider.GetService<IBaseSettings>();

            services.AddSingleton(_settings);

            services.AddAuthentication(options => { options.SignInScheme = "ServerCookie"; });

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin",
                    builder => builder.AllowAnyOrigin());
            });

            services.AddMvc()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization()
                .AddMvcOptions(o => { o.Filters.Add(typeof(UnhandledExceptionFilter)); });

            services.AddDistributedMemoryCache();

            services.AddAutoMapper();

            services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });

            WebDependencies.Create(services);

            return ApiDependencies.Create(services, _settings);
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
                if (_settings.IsDebug)
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                }

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

            app.UseCors("AllowAnyOrigin");

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

    }
}