using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Core.Registration;
using Lykke.Common.ApiLibrary.Validation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebAuth.Models;

namespace WebAuth.Controllers
{
    [Route("api/[controller]")]
    public class RegistrationController : Controller
    {
        private readonly IRegistrationRepository _registrationRepository;

        public RegistrationController(IRegistrationRepository registrationRepository)
        {
            _registrationRepository = registrationRepository;
        }

        [HttpPost]
        [SwaggerOperation("Register")]
        [ProducesResponseType(typeof(RegistrationResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> Register(RegistrationModel registrationModel)
        {
            var registrationEntity = new RegistrationInternalEntity(registrationModel);

            var registrationToken = await _registrationRepository.AddAsync(registrationEntity);

            return Json(new RegistrationResponse(registrationToken));
        }

        [HttpGet]
        [SwaggerOperation("Step")]
        [ProducesResponseType(typeof(RegistrationStepResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NotFound)]
        [Route("step/{key}")]
        [ValidateModel]
        public async Task<IActionResult> Step([Required]string key)
        {
            try
            {
                var registrationModel = await _registrationRepository.GetAsync(key);
                return Json(registrationModel.RegistrationStep);
            }
            catch (RegistrationKeyNotFoundException)
            {
                return NotFound(key);
            }
        }
    }
}
