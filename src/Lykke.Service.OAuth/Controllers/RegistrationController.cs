using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Core.Exceptions;
using Core.Registration;
using Core.Services;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Validation;
using Lykke.Common.Log;
using Lykke.Service.OAuth.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebAuth.Models;

namespace WebAuth.Controllers
{
    [Route("api/[controller]")]
    public class RegistrationController : Controller
    {
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IEmailValidationService _emailValidationService;
        private readonly ILog _log;

        public RegistrationController(
            [NotNull] IRegistrationRepository registrationRepository, 
            [NotNull] IEmailValidationService emailValidationService,
            [NotNull] ILogFactory logFactory)
        {
            _registrationRepository = registrationRepository;
            _emailValidationService = emailValidationService;
            _log = logFactory.CreateLog(this);
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

        /// <summary>
        /// Check if email is already registered
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Validation result</response>
        /// <response code="400">Email hash is invalid, BCrypt work factor is invalid, BCrypt internal exception occured, BCrypt hash format is invalid</response>
        [HttpPost]
        [Route("email")]
        [SwaggerOperation("ValidateEmail")]
        [ProducesResponseType(typeof(EmailValidationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> ValidateEmail([FromBody] ValidateEmailRequest request)
        {
            try
            {
                bool isEmailTaken = await _emailValidationService.IsEmailTakenAsync(request.Email, request.Hash);

                return Ok(new EmailValidationResult { IsEmailTaken = isEmailTaken });
            }
            catch (EmailHashInvalidException e)
            {
                _log.Warning("Invalid hash has been provided for email", e, $"email = {e.Email}");

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (BCryptWorkFactorOutOfRangeException e)
            {
                _log.Warning("BCrypt work factor is out of range", e, $"workFactor = {e.WorkFactor}");

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (BCryptInternalException e)
            {
                _log.Warning("BCrypt internal exception", e.InnerException,
                    $"email = {request.Email}, hash = {request.Hash}");

                return BadRequest(ErrorResponse.Create(e.InnerException?.Message));
            }
            catch (BCryptHashFormatException e)
            {
                _log.Warning(e.Message, e, $"hash = {e.Hash}");

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
