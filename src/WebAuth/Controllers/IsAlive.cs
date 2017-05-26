using System;
using Microsoft.AspNetCore.Mvc;

namespace WebAuth.Controllers
{
    [Route("api/[controller]")]
    public class IsAlive : Controller
    {
        [HttpGet]
        public IsAliveResponse Get()
        {
            return new IsAliveResponse
            {
                Version =
                    Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion,
                Env = Environment.GetEnvironmentVariable("ENV_INFO")
            };
        }

        public class IsAliveResponse
        {
            public string Version { get; set; }
            public string Env { get; set; }
        }
    }
}
