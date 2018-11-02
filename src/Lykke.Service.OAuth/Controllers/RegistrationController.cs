using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Common.Log;
using Core.Constants;
using Core.Exceptions;
using Core.Registration;
using Core.Services;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.ApiLibrary.Validation;
using Lykke.Common.Log;
using Lykke.Service.OAuth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        [ValidateModel]
        public async Task<IActionResult> InitialInfo([FromBody] RegistrationRequestModel registrationRequestModel)
        {
            try
            {
                var registrationModel = await _registrationRepository.GetAsync(registrationRequestModel.RegistrationId);

                registrationModel.SetInitialInfo(registrationRequestModel.ToDto());

                var registrationId = await _registrationRepository.UpdateAsync(registrationModel);

                return new JsonResult(new RegistrationResponse(registrationId));
            }
            catch (RegistrationKeyNotFoundException)
            {
                throw LykkeApiErrorException.NotFound(LykkeApiErrorCodes.RegistrationNotFound);
            }
            catch (PasswordIsPwndException)
            {
                throw LykkeApiErrorException.BadRequest(LykkeApiErrorCodes.UnsafePassword);
            }
            catch (ArgumentException)
            {
                throw LykkeApiErrorException.BadRequest(LykkeApiErrorCodes.InvalidInput);
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
        [ValidateModel]
        public async Task<IActionResult> Status([Required]string registrationId)
        {
            try
            {
                var registrationModel = await _registrationRepository.GetAsync(registrationId);
                return new JsonResult(registrationModel.RegistrationStep);
            }
            catch (RegistrationKeyNotFoundException)
            {
                throw LykkeApiErrorException.NotFound(LykkeApiErrorCodes.RegistrationNotFound);
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

                throw LykkeApiErrorException.BadRequest(LykkeApiErrorCodes.InvalidBCryptHash);
            }
            catch (BCryptWorkFactorOutOfRangeException e)
            {
                _log.Warning("BCrypt work factor is out of range", e, $"workFactor = {e.WorkFactor}");

                throw LykkeApiErrorException.BadRequest(LykkeApiErrorCodes.BCryptWorkFactorOutOfRange);
            }
            catch (BCryptInternalException e)
            {
                _log.Warning("BCrypt internal exception", e.InnerException,
                    $"email = {request.Email}, hash = {request.Hash}");

                throw LykkeApiErrorException.BadRequest(LykkeApiErrorCodes.BCryptInternalError);
            }
            catch (BCryptHashFormatException e)
            {
                _log.Warning(e.Message, e, $"hash = {e.Hash}");

                throw LykkeApiErrorException.BadRequest(LykkeApiErrorCodes.InvalidBCryptHashFormat);
            }
        }
    }
}
