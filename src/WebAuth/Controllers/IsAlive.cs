using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;

namespace WebAuth.Controllers
{
    [Route("api/[controller]")]
    public class IsAlive : Controller
    {
        [HttpGet]
        public string Get()
        {
            var response = new IsAliveResponse
            {
                Name = PlatformServices.Default.Application.ApplicationName,
                Version = PlatformServices.Default.Application.ApplicationVersion,
                Env = Environment.GetEnvironmentVariable("ENV_INFO")
            };

            return JsonConvert.SerializeObject(response);
        }

        public class IsAliveResponse
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public string Env { get; set; }
        }
    }
}
