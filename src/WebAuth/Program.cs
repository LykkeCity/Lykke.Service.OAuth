using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Net;
using AzureStorage.Blob;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Common;
using Lykke.SettingsReader.ReloadingManager;
using Microsoft.Extensions.PlatformAbstractions;

namespace WebAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"{PlatformServices.Default.Application.ApplicationName} version {PlatformServices.Default.Application.ApplicationVersion}");
            //#$if DEBUG
            Console.WriteLine("Is DEBUG");
            //#$else
            //$#$//Console.WriteLine("Is RELEASE");
            //#$endif           
            try
            {
                {
                    var host = new WebHostBuilder()
                        .UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseApplicationInsights()
                        .UseUrls("http://*:5000/")
                        .UseStartup<Startup>()
                        .Build();

                    host.Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error:");
                Console.WriteLine(ex);

                // Lets devops to see startup error in console between restarts in the Kubernetes
                var delay = TimeSpan.FromMinutes(1);

                Console.WriteLine();
                Console.WriteLine($"Process will be terminated in {delay}. Press any key to terminate immediately.");

                Task.WhenAny(
                        Task.Delay(delay),
                        Task.Run(() =>
                        {
                            Console.ReadKey(true);
                        }))
                    .Wait();
            }

            Console.WriteLine("Terminated");
        }
    }
}
