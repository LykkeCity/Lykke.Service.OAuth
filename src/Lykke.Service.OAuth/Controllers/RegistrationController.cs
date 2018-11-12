using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Core;
using Core.Application;
using Core.Countries;
using Core.Exceptions;
using Core.PasswordValidation;
using Core.Registration;
using Core.Services;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Service.OAuth.Attributes;
using Lykke.Service.OAuth.Models;
using Lykke.Service.OAuth.Models.Registration;
using Lykke.Service.OAuth.Services.Countries;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

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
        private readonly IApplicationRepository _applicationRepository;
        private readonly ICountriesService _countriesService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="registrationRepository"></param>
        /// <param name="emailValidationService"></param>
        /// <param name="passwordValidationService"></param>
        /// <param name="applicationRepository"></param>
        /// <param name="countriesService"></param>
        public RegistrationController(
            IRegistrationRepository registrationRepository, 
            IEmailValidationService emailValidationService,
            IPasswordValidationService passwordValidationService,
            IApplicationRepository applicationRepository, 
            ICountriesService countriesService)
        {
            _applicationRepository = applicationRepository;
            _countriesService = countriesService;
            _registrationRepository = registrationRepository;
            _emailValidationService = emailValidationService;
            _passwordValidationService = passwordValidationService;
        }

        /// <summary>
        /// Submits initial info of the user
        /// </summary>
        /// <param name="registrationRequestModel"></param>
        /// <response code="200">The id of the registration has been started</response>
        /// <response code="400">Request validation failed. Error codes: PasswordIsPwned, PasswordIsNotComplex, PasswordIsEmpty</response>
        /// <response code="404">Registration id not found. Error codes: RegistrationNotFound, ClientNotFound</response>
        [HttpPost]
        [SwaggerOperation("InitialInfo")]
        [ProducesResponseType(typeof(RegistrationResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.NotFound)]
        [Route("initialInfo")]
        [ValidateApiModel]
        public async Task<IActionResult> InitialInfo([FromBody] InitialInfoRequestModel registrationRequestModel)
        {
            if (await _applicationRepository.GetByIdAsync(registrationRequestModel.ClientId) == null)
                throw new ClientNotFoundException(registrationRequestModel.ClientId);

            var registrationModel =
                await _registrationRepository.GetByIdAsync(registrationRequestModel.RegistrationId);

            var passwordValidationResult =
                await _passwordValidationService.ValidateAsync(registrationRequestModel.Password);

            passwordValidationResult.ThrowOrKeepSilent();

            registrationModel.CompleteInitialInfoStep(registrationRequestModel.ToDto());

            var registrationId = await _registrationRepository.UpdateAsync(registrationModel);

            return new JsonResult(new RegistrationResponse(registrationId));
        }

        /// <summary>
        /// Submits account information step
        /// </summary>
        /// <param name="model"></param>
        /// <response code="200">The id of the registration has been proceeded to the next step</response>
        /// <response code="400">Invalid country or phone number or phone number is already used. Error codes:ModelValidationFailed, CountryFromRestrictedList, CountryCodeInvalid, InvalidPhoneFormat, PhoneNumberInUse</response>
        /// <response code="404">Registration id not found. Error codes: RegistrationNotFound</response>
        [HttpPost]
        [Route("accountInfo")]
        [SwaggerOperation("AccountInfo")]
        [ProducesResponseType(typeof(RegistrationResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateApiModel]
        public async Task<IActionResult> AccountInfo([FromBody] AccountInfoRequestModel model)
        {
            RegistrationModel registrationModel = await _registrationRepository.GetByIdAsync(model.RegistrationId);

            _countriesService.ValidateCountryCode(model.CountryCodeIso2);

            //todo: validate if phone number is already in use using old KYC database?

            registrationModel.CompleteAccountInfoStep(model.ToDto());

            var registrationId = await _registrationRepository.UpdateAsync(registrationModel);

            return Ok(new RegistrationResponse(registrationId));

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
            var registrationModel = await _registrationRepository.GetByIdAsync(registrationId);

            return new JsonResult(new RegistrationStatusResponse
            {
                RegistrationStep = registrationModel.CurrentStep
            });
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
            var isEmailTaken = await _emailValidationService.IsEmailTakenAsync(request.Email, request.Hash);

            if (!isEmailTaken)
            {
                var registrationModel = new RegistrationModel(request.Email);
                var registrationId = await _registrationRepository.AddAsync(registrationModel);
                return Ok(new EmailValidationResult {IsEmailTaken = false, RegistrationId = registrationId});
            }

            return Ok(new EmailValidationResult {IsEmailTaken = true});
        }

        /// <summary>
        ///     Get list of countries.
        ///     And list of restricted countries of residence.
        /// </summary>
        /// <response code="200">
        ///     List of countries.
        ///     And list of restricted countries of residence.
        /// </response>
        [HttpGet]
        [Route("countries")]
        [SwaggerOperation("GetCountries")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CountriesResponse), (int) HttpStatusCode.OK)]
        [ValidateApiModel]
        public IActionResult GetCountries()
        {
            return new JsonResult(
                new CountriesResponse(
                    _countriesService.Countries,
                    _countriesService.RestrictedCountriesOfResidence));
        }
    }
}
