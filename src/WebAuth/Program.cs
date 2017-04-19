using System;
using System.IO;
using AzureDataAccess.Settings;
using Lykke.Common;
using Core.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Core.Validation;

namespace WebAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var settings = ReadGeneralSettings();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureServices(collection => collection.AddSingleton<IBaseSettings>(settings))
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }

        private static IBaseSettings ReadGeneralSettings()
        {
            var settingsData = ReadSettingsFile();

            if (string.IsNullOrWhiteSpace(settingsData))
            {
                throw new Exception("Please, provide generalsettings.json file");
            }

            var settings = GeneralSettingsReader.ReadSettingsFromData<BaseSettings>(settingsData);

            GeneralSettingsValidator.Validate(settings);

            return settings;
        }

        private static string ReadSettingsFile()
        {
            try
            {
                string path;
#if DEBUG
                path = @"generalsettings.json";
#else
                path = "generalsettings.json";
#endif

                return File.ReadAllText(path);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
