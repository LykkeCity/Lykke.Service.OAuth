using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    public class IsAlive : Controller
    {
        [HttpGet]
        public string Get()
        {
            var response = new IsAliveResponse
            {
                Version =
                    Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion,
                Env = Environment.GetEnvironmentVariable("ENV_INFO")
            };

            return JsonConvert.SerializeObject(response);
        }

        public class IsAliveResponse
        {
            public string Version { get; set; }
            public string Env { get; set; }
        }
    }
}
