using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Net;
using AzureStorage.Blob;
using System.Security.Cryptography.X509Certificates;
using Common;

namespace WebAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var sertConnString = Environment.GetEnvironmentVariable("CertConnectionString");

            if (string.IsNullOrWhiteSpace(sertConnString) || sertConnString.Length < 10)
            {

                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseUrls("http://*:5000/")
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();

                host.Run();

            }
            else
            {
                var sertContainer = Environment.GetEnvironmentVariable("CertContainer");
                var sertFilename = Environment.GetEnvironmentVariable("CertFileName");
                var sertPassword = Environment.GetEnvironmentVariable("CertPassword");

                var certBlob = new AzureBlobStorage(sertConnString);
                var cert = certBlob.GetAsync(sertContainer, sertFilename).Result.ToBytes();
                
                X509Certificate2 xcert = new X509Certificate2(cert, sertPassword);

                var host = new WebHostBuilder()
                    .UseKestrel(x =>
                    {
                        x.AddServerHeader = false;
                        x.Listen(IPAddress.Loopback, 443, listenOptions =>
                        {
                            listenOptions.UseHttps(xcert);
                            listenOptions.UseConnectionLogging();
                        });

                    })
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseUrls("https://*:443/")
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();

                host.Run();
            }
        }
    }
}

