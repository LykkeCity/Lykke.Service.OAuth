using System.Net;
using Lykke.Service.OAuth.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAuth.Settings.ServiceSettings;

namespace Lykke.Service.OAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly SecuritySettings _securitySettings;

        public SettingsController(SecuritySettings securitySettings)
        {
            _securitySettings = securitySettings;
        }

        /// <summary>
        /// Get registration settings
        /// </summary>
        /// <response code="200">Registration settings details</response>
        [HttpGet]
        [Route("registration")]
        [SwaggerOperation("GetRegistrationSettings")]
        [ProducesResponseType(typeof(RegistrationSettingsResponse), (int) HttpStatusCode.OK)]
        public IActionResult GetRegistrationSettings()
        {
            return Ok(new RegistrationSettingsResponse {BCryptWorkFactor = _securitySettings.BCryptWorkFactor});
        }
    }
}
