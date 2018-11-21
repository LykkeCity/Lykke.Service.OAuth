using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Log;
using Core.Application;
using Core.Countries;
using Core.Exceptions;
using Core.Extensions;
using Core.PasswordValidation;
using Core.Registration;
using Core.Services;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Extensions;
using Lykke.Common.Log;
using Lykke.Service.OAuth.ApiErrorCodes;
using Lykke.Service.OAuth.Attributes;
using Lykke.Service.OAuth.Models;
using Lykke.Service.OAuth.Models.Registration;
using Lykke.Service.OAuth.Models.Registration.Countries;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.Registration;
using Lykke.Service.Registration.Contract.Client.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebAuth.Managers;

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
        private readonly IPersonalDataService _personalDataService;
        private readonly ILog _log;
        private readonly IRegistrationServiceClient _registrationServiceClient;
        private readonly IUserManager _userManager;

        public RegistrationController(
            IRegistrationRepository registrationRepository, 
            IEmailValidationService emailValidationService,
            IPasswordValidationService passwordValidationService,
            IApplicationRepository applicationRepository, 
            ICountriesService countriesService,
            ILogFactory logFactory, 
            IPersonalDataService personalDataService,
            IRegistrationServiceClient registrationServiceClient,
            IUserManager userManager
            )
        {
            _userManager = userManager;
            _registrationServiceClient = registrationServiceClient;
            _applicationRepository = applicationRepository;
            _countriesService = countriesService;
            _personalDataService = personalDataService;
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
        /// <response code="400">Request validation failed. Error codes: PasswordIsPwned, PasswordIsNotComplex, PasswordIsEmpty</response>
        /// <response code="404">
        /// When RegistrationId is null, empty or whitespace.
        /// Registration id not found. Error codes: RegistrationNotFound,
        ///
        /// When Id of the client app was not found:
        /// Error code: ClientNotFound
        /// </response>
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

            await _passwordValidationService.ValidateAndThrowAsync(registrationRequestModel.Password);

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
        /// <response code="404">
        ///     When RegistrationId is null, empty or whitespace.
        ///     When Registration id not found.
        ///     Error codes: RegistrationNotFound
        /// </response>
        [HttpPost]
        [Route("accountInfo")]
        [SwaggerOperation("AccountInfo")]
        [ProducesResponseType(typeof(RegistrationCompleteResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateApiModel]
        public async Task<IActionResult> AccountInfo([FromBody] AccountInfoRequestModel model)
        {
            var registrationModel = await _registrationRepository.GetByIdAsync(model.RegistrationId);

            _countriesService.ValidateCode(model.CountryCodeIso2);

            var searchPersonalDataModel =
                await _personalDataService.FindClientsByPhrase(model.PhoneNumber, SearchMode.Term);

            if (searchPersonalDataModel != null)
                throw new PhoneNumberAlreadyInUseException(model.PhoneNumber);

            registrationModel.CompleteAccountInfoStep(model.ToDto());

            await _registrationRepository.UpdateAsync(registrationModel);

            var registrationServiceResponse = await CreateUserAsync(registrationModel);

            //todo: @mkobzev fix nre
            //await SignInAsync(registrationServiceResponse, registrationModel);

            return Ok(
                new RegistrationCompleteResponse(registrationServiceResponse.Token,
                    registrationServiceResponse.NotificationsId)
            );
        }

        private async Task SignInAsync(AccountsRegistrationResponseModel registrationServiceResponse,
            RegistrationModel registrationModel)
        {
            var identity = await _userManager.CreateUserIdentityAsync(registrationServiceResponse.Account.Id,
                registrationModel.Email, registrationModel.Email, null, registrationServiceResponse.Token, true);

            await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme, new ClaimsPrincipal(identity));
        }

        private async Task<AccountsRegistrationResponseModel> CreateUserAsync(RegistrationModel registrationModel)
        {
            var registrationServiceResponse = await _registrationServiceClient.RegistrationApi.RegisterAsync(
                new SafeAccountRegistrationModel
                {
                    Email = registrationModel.Email,
                    ClientId = registrationModel.ClientId,
                    CountryFromPOA = registrationModel.CountryOfResidenceIso2,
                    FullName = registrationModel.FirstName + " " + registrationModel.LastName,
                    ContactPhone = registrationModel.PhoneNumber,
                    ClientInfo = null,
                    Changer = null,
                    PartnerId = null,
                    Salt = registrationModel.Salt,
                    Hash = registrationModel.Hash,
                    Cid = null,
                    CreatedAt = registrationModel.Started,
                    Hint = null,
                    IosVersion = null,
                    Ttl = TimeSpan.FromDays(3),
                    Ip = HttpContext.GetIp(),
                    UserAgent = HttpContext.GetUserAgent(),
                    RegistrationId = registrationModel.RegistrationId
                }
            );
            return registrationServiceResponse;
        }

        /// <summary>
        /// Returns the status of registration by a provided key
        /// </summary>
        /// <param name="registrationId">The id of registration</param>
        /// <response code="200">The current state of registration</response>
        /// <response code="400">Request validation failed</response>
        /// <response code="404">Registration id not found. Error codes: RegistrationNotFound</response>
        [Obsolete("This method would be removed as it considered unsafe to transfer registrationId. Please use POST instead.")]
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
        ///     Returns the status of registration
        /// </summary>
        /// <param name="request">Registration status request object.</param>
        /// <response code="200">The current state of registration</response>
        /// <response code="404">
        ///     When RegistrationId is null, empty or whitespace.
        ///     When Registration id not found.
        ///     Error codes: RegistrationNotFound
        /// </response>
        [HttpPost]
        [SwaggerOperation("Status")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(RegistrationStatusResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.NotFound)]
        [Route("status")]
        [ValidateApiModel]
        public async Task<IActionResult> Status([FromBody] RegistrationStatusRequest request)
        {
            try
            {
                var registrationModel = await _registrationRepository.GetByIdAsync(request.RegistrationId);

                return new JsonResult(new RegistrationStatusResponse
                {
                    RegistrationStep = registrationModel.CurrentStep
                });
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
            var isEmailTaken = await _emailValidationService.IsEmailTakenAsync(request.Email, request.Hash);

            if (!isEmailTaken)
            {
                var registrationModel = new RegistrationModel(request.Email, DateTime.UtcNow);
                var registrationId = await _registrationRepository.AddAsync(registrationModel);
                return Ok(new EmailValidationResult {IsEmailTaken = false, RegistrationId = registrationId});
            }

            return Ok(new EmailValidationResult {IsEmailTaken = true});
        }

        /// <summary>
        ///     Get countries information for registration process.
        /// </summary>
        /// <param name="request">Request object for countries.</param>
        /// <remarks>
        ///     If country could not be detected by provided ip, there will be no error.
        ///     userLocationCountry would be null.
        ///     We don't want to block user registration, because of we can't autodetect his country.
        /// </remarks>
        /// <response code="200">
        ///     List of countries.
        ///     And list of restricted countries of residence.
        ///     And user location country.
        /// </response>
        /// <response code="404">
        ///     When RegistrationId is null, empty or whitespace.
        ///     When Registration id not found.
        ///     Error codes: RegistrationNotFound
        /// </response>
        [HttpPost]
        [Route("countries")]
        [SwaggerOperation("GetCountries")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CountriesResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateApiModel]
        public async Task<IActionResult> GetCountries([FromBody] CountriesRequest request)
        {
            await _registrationRepository.GetByIdAsync(request.RegistrationId);

            CountryInfo userLocationCountry = null;

            try
            {
                userLocationCountry = await _countriesService.GetCountryByIpAsync(HttpContext.GetIp());
            }
            catch (CountryNotFoundException e)
            {
                _log.Warning(e.Message, e);
            }

            return new JsonResult(
                new CountriesResponse(
                    _countriesService.Countries,
                    _countriesService.RestrictedCountriesOfResidence,
                    userLocationCountry));
        }
    }
}
