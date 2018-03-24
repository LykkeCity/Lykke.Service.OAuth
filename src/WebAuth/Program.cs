using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Common;
using AzureStorage.Blob;
using Lykke.SettingsReader.ReloadingManager;

namespace WebAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
                        .Build();

                    host.Run();

                }
                else
                {
                    var sertContainer = Environment.GetEnvironmentVariable("CertContainer");
                    var sertFilename = Environment.GetEnvironmentVariable("CertFileName");
                    var sertPassword = Environment.GetEnvironmentVariable("CertPassword");

                    var settingsManager = ConstantReloadingManager.From(sertConnString);
                    var certBlob = AzureBlobStorage.Create(settingsManager);
                    var cert = certBlob.GetAsync(sertContainer, sertFilename).GetAwaiter().GetResult().ToBytes();

                    X509Certificate2 xcert = new X509Certificate2(cert, sertPassword);

                    var host = new WebHostBuilder()
                        .UseKestrel(x =>
                        {
                            x.Listen(IPAddress.Any, 443, o => o.UseHttps(xcert));
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
