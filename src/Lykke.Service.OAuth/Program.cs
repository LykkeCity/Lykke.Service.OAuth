using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.Extensions.PlatformAbstractions;

namespace WebAuth
{
    internal static class Program
    {
        public static string EnvInfo => Environment.GetEnvironmentVariable("ENV_INFO");

        public static async Task Main(string[] args)
        {

            Console.WriteLine($"{PlatformServices.Default.Application.ApplicationName} version {PlatformServices.Default.Application.ApplicationVersion}");

            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error:");
                Console.WriteLine(ex);

                // Lets devops to see startup error in console between restarts in the Kubernetes
                var delay = TimeSpan.FromMinutes(1);

                Console.WriteLine();
                Console.WriteLine($"Process will be terminated in {delay}. Press any key to terminate immediately.");

                await Task.WhenAny(
                        Task.Delay(delay),
                    Task.Run(() => { Console.ReadKey(true); }));
            }

            Console.WriteLine("Terminated");
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
#if !DEBUG
                .UseApplicationInsights()
                .UseUrls("http://*:5000/")
#endif
                .UseStartup<Startup>();
    }
}
