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
    /// <summary>
    /// Registration-related stuff
    /// </summary>
    [Route("api/[controller]")]
    public class RegistrationController : Controller
    {
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IEmailValidationService _emailValidationService;
        private readonly ILog _log;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="registrationRepository"></param>
        /// <param name="emailValidationService"></param>
        /// <param name="logFactory"></param>
        public RegistrationController(
            [NotNull] IRegistrationRepository registrationRepository, 
            [NotNull] IEmailValidationService emailValidationService,
            [NotNull] ILogFactory logFactory)
        {
            _registrationRepository = registrationRepository;
            _emailValidationService = emailValidationService;
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        /// Starts registration proccess of the user
        /// </summary>
        /// <param name="registrationRequestModel"></param>
        /// <response code="200">The id of the registration has been started</response>
        /// <response code="400">Request validation failed</response>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Register")]
        [ProducesResponseType(typeof(RegistrationResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> Register([FromBody]RegistrationRequestModel registrationRequestModel)
        {
            var registrationModel = new RegistrationModel(registrationRequestModel.ToDomain());

            //todo: @mkobzev add one more time validation of existing email and the check for pwned passwords
            var isValid = true;
            if (isValid)
            {
                registrationModel.SetInitialInfoAsValid();

                var registrationId = await _registrationRepository.AddAsync(registrationModel);

                return Json(new RegistrationResponse(registrationId));
            }

            return BadRequest();
        }

        /// <summary>
        /// Returns the status of registration by a provided key
        /// </summary>
        /// <param name="registrationId">The id of registration</param>
        /// <response code="200">The current state of registration</response>
        /// <response code="400">Request validation failed</response>
        /// <response code="404">Registration with such an id was not fount</response>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Status")]
        [ProducesResponseType(typeof(RegistrationStatusResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NotFound)]
        [Route("status/{registrationId}")]
        [ValidateModel]
        public async Task<IActionResult> Status([Required]string registrationId)
        {
            try
            {
                var registrationModel = await _registrationRepository.GetAsync(registrationId);
                return Json(registrationModel.RegistrationStep);
            }
            catch (RegistrationKeyNotFoundException)
            {
                return NotFound(registrationId);
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
