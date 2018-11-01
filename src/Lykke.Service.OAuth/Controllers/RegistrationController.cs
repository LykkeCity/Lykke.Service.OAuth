using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Core.Exceptions;
using Core.PasswordValidation;
using Core.Registration;
using Core.Services;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using Lykke.Service.OAuth.ApiErrorCodes;
using Lykke.Service.OAuth.Attributes;
using Lykke.Service.OAuth.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebAuth.Models;

namespace Lykke.Service.OAuth.Controllers
{
    /// <summary>
    /// Registration-related stuff
    /// </summary>
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IEmailValidationService _emailValidationService;
        private readonly IPasswordValidationService _passwordValidationService;
        private readonly ILog _log;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="registrationRepository"></param>
        /// <param name="emailValidationService"></param>
        /// <param name="passwordValidationService"></param>
        /// <param name="logFactory"></param>
        public RegistrationController(
            [NotNull] IRegistrationRepository registrationRepository, 
            [NotNull] IEmailValidationService emailValidationService,
            IPasswordValidationService passwordValidationService,
            [NotNull] ILogFactory logFactory)
        {
            _registrationRepository = registrationRepository;
            _emailValidationService = emailValidationService;
            _passwordValidationService = passwordValidationService;
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        /// Submits initial info of the user
        /// </summary>
        /// <param name="registrationRequestModel"></param>
        /// <response code="200">The id of the registration has been started</response>
        /// <response code="400">Request validation failed</response>
        /// <response code="404">Registration id not found</response>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("InitialInfo")]
        [ProducesResponseType(typeof(RegistrationResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [Route("initialInfo")]
        [ValidateApiModel]
        public async Task<IActionResult> InitialInfo([FromBody] RegistrationRequestModel registrationRequestModel)
        {
            try
            {
                var passwordValidationResult = await _passwordValidationService.ValidateAsync(registrationRequestModel.Password);
                                
                if (!passwordValidationResult.IsValid)
                {
                    var apiError = PasswordValidationApiErrorCodes.GetApiErrorByValidationErrorCode(passwordValidationResult.Error);
                    throw LykkeApiErrorException.BadRequest(apiError);
                }

                var registrationModel = await _registrationRepository.GetAsync(registrationRequestModel.RegistrationId);


                registrationModel.SetInitialInfo(registrationRequestModel.ToDto());

                var registrationId = await _registrationRepository.UpdateAsync(registrationModel);

                return new JsonResult(new RegistrationResponse(registrationId));
            }
            catch (RegistrationKeyNotFoundException)
            {
                return NotFound(ErrorResponse.Create(registrationRequestModel.RegistrationId));
            }
            catch (PasswordIsPwndException)
            {
                return BadRequest(ErrorResponse.Create("Please, try to choose another password. This one is unsafe."));
            }
            catch (ArgumentException e)
            {
                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Returns the status of registration by a provided key
        /// </summary>
        /// <param name="registrationId">The id of registration</param>
        /// <response code="200">The current state of registration</response>
        /// <response code="400">Request validation failed</response>
        /// <response code="404">Registration id not found</response>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Status")]
        [ProducesResponseType(typeof(RegistrationStatusResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [Route("status/{registrationId}")]
        [ValidateApiModel]
        public async Task<IActionResult> Status([Required]string registrationId)
        {
            try
            {
                var registrationModel = await _registrationRepository.GetAsync(registrationId);
                return new JsonResult(registrationModel.RegistrationStep);
            }
            catch (RegistrationKeyNotFoundException)
            {
                return NotFound(ErrorResponse.Create(registrationId));
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
        [ValidateApiModel]
        public async Task<IActionResult> ValidateEmail([FromBody] ValidateEmailRequest request)
        {
            try
            {
                var isEmailTaken = await _emailValidationService.IsEmailTakenAsync(request.Email, request.Hash);

                if (!isEmailTaken)
                {
                    var registrationModel = new RegistrationModel(request.Email);
                    var registrationId = await _registrationRepository.AddAsync(registrationModel);
                    return Ok(new EmailValidationResult { IsEmailTaken = false, RegistrationId = registrationId });
                }

                return Ok(new EmailValidationResult { IsEmailTaken = true });
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
