using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Core.Application;
using Core.Exceptions;
using Core.PasswordValidation;
using Core.Registration;
using Core.Services;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Contract;
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
        private readonly IApplicationRepository _applicationRepository;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="registrationRepository"></param>
        /// <param name="emailValidationService"></param>
        /// <param name="passwordValidationService"></param>
        /// <param name="logFactory"></param>
        /// <param name="applicationRepository"></param>
        public RegistrationController(
            IRegistrationRepository registrationRepository, 
            IEmailValidationService emailValidationService,
            IPasswordValidationService passwordValidationService,
            ILogFactory logFactory,
            IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
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
        /// <response code="400">Request validation failed. Error codes: PasswordIsPwned, PasswordIsNotComplex</response>
        /// <response code="404">Registration id not found. Error codes: RegistrationNotFound, ClientNotFound</response>
        [HttpPost]
        [SwaggerOperation("InitialInfo")]
        [ProducesResponseType(typeof(RegistrationResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.NotFound)]
        [Route("initialInfo")]
        [ValidateApiModel]
        public async Task<IActionResult> InitialInfo([FromBody] RegistrationRequestModel registrationRequestModel)
        {
            try
            {
                var client = await _applicationRepository.GetByIdAsync(registrationRequestModel.ClientId);
                if (client == null)
                    throw LykkeApiErrorException.NotFound(OAuthErrorCodes.ClientNotFound);
                    
                var registrationModel = await _registrationRepository.GetByIdAsync(registrationRequestModel.RegistrationId);

                var passwordValidationResult =
                    await _passwordValidationService.ValidateAsync(registrationRequestModel.Password);
                    
                if (!passwordValidationResult.IsValid)
                {
                    var apiError =
                        PasswordValidationApiErrorCodes.GetApiErrorCodeByValidationErrorCode(passwordValidationResult
                            .Error);
                    throw LykkeApiErrorException.BadRequest(apiError);
                }

                registrationModel.SetInitialInfo(registrationRequestModel.ToDto());

                var registrationId = await _registrationRepository.UpdateAsync(registrationModel);

                return new JsonResult(new RegistrationResponse(registrationId));
            }
            catch (RegistrationKeyNotFoundException)
            {
                throw LykkeApiErrorException.NotFound(RegistrationErrorCodes.RegistrationNotFound);
            }
            catch (PasswordIsNotComplexException)
            {
                var apiError = PasswordValidationApiErrorCodes.GetApiErrorCodeByValidationErrorCode(
                    PasswordValidationErrorCode.PasswordIsNotComplex);

                throw LykkeApiErrorException.BadRequest(apiError);
            }
        }

        /// <summary>
        /// Returns the status of registration by a provided key
        /// </summary>
        /// <param name="registrationId">The id of registration</param>
        /// <response code="200">The current state of registration</response>
        /// <response code="400">Request validation failed</response>
        /// <response code="404">Registration id not found. Error codes: RegistrationNotFound</response>
        [HttpGet]
        [SwaggerOperation("Status")]
        [ProducesResponseType(typeof(RegistrationStatusResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.NotFound)]
        [Route("status/{registrationId}")]
        [ValidateApiModel]
        public async Task<IActionResult> Status([Required] string registrationId)
        {
            try
            {
                var registrationModel = await _registrationRepository.GetByIdAsync(registrationId);
                return new JsonResult(registrationModel.RegistrationStep);
            }
            catch (RegistrationKeyNotFoundException)
            {
                throw LykkeApiErrorException.NotFound(RegistrationErrorCodes.RegistrationNotFound);
            }
        }

        /// <summary>
        /// Check if email is already registered
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Validation result</response>
        /// <response code="400">Email hash is invalid, BCrypt work factor is invalid, BCrypt internal exception occured, BCrypt hash format is invalid. Error codes: InvalidBCryptHash, BCryptWorkFactorOutOfRange, BCryptInternalError, InvalidBCryptHashFormat </response>
        [HttpPost]
        [Route("email")]
        [SwaggerOperation("ValidateEmail")]
        [ProducesResponseType(typeof(EmailValidationResult), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.BadRequest)]
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
                    return Ok(new EmailValidationResult {IsEmailTaken = false, RegistrationId = registrationId});
                }

                return Ok(new EmailValidationResult {IsEmailTaken = true});
            }
            catch (EmailHashInvalidException e)
            {
                _log.Warning("Invalid hash has been provided for email", e, $"email = {e.Email}");

                throw LykkeApiErrorException.BadRequest(RegistrationErrorCodes.InvalidBCryptHash);
            }
            catch (BCryptWorkFactorOutOfRangeException e)
            {
                _log.Warning("BCrypt work factor is out of range", e, $"workFactor = {e.WorkFactor}");

                throw LykkeApiErrorException.BadRequest(RegistrationErrorCodes.BCryptWorkFactorOutOfRange);
            }
            catch (BCryptInternalException e)
            {
                _log.Warning("BCrypt internal exception", e.InnerException,
                    $"email = {request.Email}, hash = {request.Hash}");

                throw LykkeApiErrorException.BadRequest(RegistrationErrorCodes.BCryptInternalError);
            }
            catch (BCryptHashFormatException e)
            {
                _log.Warning(e.Message, e, $"hash = {e.Hash}");

                throw LykkeApiErrorException.BadRequest(RegistrationErrorCodes.InvalidBCryptHashFormat);
            }
        }
    }
}
