using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Net;
using AzureStorage.Blob;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Common;
using Lykke.SettingsReader.ReloadingManager;
using Microsoft.Extensions.Logging;
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
            var sertConnString = Environment.GetEnvironmentVariable("CertConnectionString");

            try
            {
                if (string.IsNullOrWhiteSpace(sertConnString) || sertConnString.Length < 10)
                {
                    var host = new WebHostBuilder()
                        .UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseApplicationInsights()
                        .UseUrls("http://*:5000/")
                        .UseStartup<Startup>()
                        .ConfigureLogging((hostingContext, logging) =>
                        {
                            logging.AddConsole();
                            logging.AddFilter("AspNet", LogLevel.Trace);
                            logging.SetMinimumLevel(LogLevel.Error);
                        })
                        .Build();

                    host.Run();
                }
                else
                {
                    var sertContainer = Environment.GetEnvironmentVariable("CertContainer");
                    var sertFilename = Environment.GetEnvironmentVariable("CertFileName");
                    var sertPassword = Environment.GetEnvironmentVariable("CertPassword");

                    var certBlob = AzureBlobStorage.Create(ConstantReloadingManager.From(sertConnString));
                    var cert = certBlob.GetAsync(sertContainer, sertFilename).Result.ToBytes();

                    X509Certificate2 xcert = new X509Certificate2(cert, sertPassword);

                    var host = new WebHostBuilder()
                        .UseKestrel(x =>
                        {
                            x.Listen(IPAddress.Any, 443, listenOptions => listenOptions.UseHttps(xcert));
                            x.AddServerHeader = false;
                        })
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseApplicationInsights()
                        .UseUrls("https://*:443/")
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
