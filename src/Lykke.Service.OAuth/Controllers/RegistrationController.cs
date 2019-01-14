using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using Common.Log;
using Core.Application;
using Core.Countries;
using Core.Exceptions;
using Core.Extensions;
using Core.ExternalProvider;
using Core.ExternalProvider.Settings;
using Core.PasswordValidation;
using Core.Registration;
using Core.Services;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Extensions;
using Lykke.Common.Log;
using Lykke.Service.OAuth.ApiErrorCodes;
using Lykke.Service.OAuth.Attributes;
using Lykke.Service.OAuth.Factories;
using Lykke.Service.OAuth.Models;
using Lykke.Service.OAuth.Models.Registration;
using Lykke.Service.OAuth.Models.Registration.Countries;
using Lykke.Service.OAuth.Settings;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.Registration;
using Lykke.Service.Registration.Contract.Client.Models;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAuth.Managers;

namespace Lykke.Service.OAuth.Controllers
{
    /// <summary>
    /// Registration-related stuff
    /// </summary>
    [Route("api/[controller]")]
    [TypeFilter(typeof(FeatureToggleFilter), Arguments = new object [] { Features.Registration })]
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
        private readonly ISalesforceService _salesforceService;
        private readonly IRequestModelFactory _requestModelFactory;
        private readonly RedirectSettings _redirectSettings;
        private readonly IExternalUserOperator _externalUserOperator;

        public RegistrationController(
            IRegistrationRepository registrationRepository, 
            IEmailValidationService emailValidationService,
            IPasswordValidationService passwordValidationService,
            IApplicationRepository applicationRepository, 
            ICountriesService countriesService,
            ILogFactory logFactory, 
            IPersonalDataService personalDataService,
            IRegistrationServiceClient registrationServiceClient,
            IUserManager userManager,
            ISalesforceService salesforceService, 
            IRequestModelFactory requestModelFactory,
            IRedirectSettingsAccessor redirectSettingsAccessor,
            IExternalUserOperator externalUserOperator
            )
        {
            _externalUserOperator = externalUserOperator;
            _userManager = userManager;
            _salesforceService = salesforceService;
            _requestModelFactory = requestModelFactory;
            _registrationServiceClient = registrationServiceClient;
            _applicationRepository = applicationRepository;
            _countriesService = countriesService;
            _personalDataService = personalDataService;
            _registrationRepository = registrationRepository;
            _emailValidationService = emailValidationService;
            _passwordValidationService = passwordValidationService;
            _redirectSettings = redirectSettingsAccessor.RedirectSettings;
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
            
            _salesforceService.CreateContact(registrationRequestModel.Email);

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
        [ProducesResponseType(typeof(RegistrationCompleteResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int)HttpStatusCode.NotFound)]
        [ValidateApiModel]
        [Consumes("application/json")]
        public async Task<ActionResult> AccountInfo([FromBody] AccountInfoRequestModel model)
        {
            return await HandleAccountInfo(model);
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
        [ProducesResponseType(typeof(RegistrationCompleteResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(LykkeApiErrorResponse), (int)HttpStatusCode.NotFound)]
        [ValidateApiModel]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult> AccountInfoForm([FromForm] AccountInfoRequestModel model)
        {
            return await HandleAccountInfo(model);
        }

        private async Task<ActionResult> HandleAccountInfo(AccountInfoRequestModel model)
        {
            var registrationModel = await _registrationRepository.GetByIdAsync(model.RegistrationId);

            _countriesService.ValidateCode(model.CountryCodeIso2);

            var searchPersonalDataModel =
                await _personalDataService.FindClientsByPhrase(model.PhoneNumber, SearchMode.Term);

            if (searchPersonalDataModel != null)
                throw new PhoneNumberAlreadyInUseException(model.PhoneNumber);

            registrationModel.CompleteAccountInfoStep(model.ToDto());

            await _registrationRepository.UpdateAsync(registrationModel);

            var registrationServiceResponse = await CreateUserAsync(registrationModel, model.Cid);

            if (registrationServiceResponse == null)
            {
                throw new Exception("Null response from registration service during registration.");
            }

            await _externalUserOperator.SaveTempLykkeUserIdAsync(registrationServiceResponse.Account.Id,
                OpenIdConnectConstantsExt.Cookies.RegistrationRequestCookie);

            //await SignInAsync(registrationServiceResponse, registrationModel);

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("ExternalLoginCallback", "External")
            };

            properties.SetProperty(
                OpenIdConnectConstantsExt.AuthenticationProperties.ExternalLoginRedirectUrl,
                Url.Action("GetLykkeWalletTokenMobile", "Userinfo")
            );

            properties.SetProperty(
                OpenIdConnectConstantsExt.AuthenticationProperties.AcrValues,
                _redirectSettings.OldLykkeSignInIroncladAuthAcrValues
            );

            properties.SetProperty(
                OpenIdConnectConstantsExt.AuthenticationProperties.RegistrationRedirectUrl,
                Url.Action("GetLykkeWalletTokenMobile", "Userinfo")
            );

            return Challenge(properties, OpenIdConnectConstantsExt.Auth.IroncladAuthenticationScheme);
        }

        private async Task SignInAsync(AccountsRegistrationResponseModel registrationServiceResponse,
            RegistrationModel registrationModel)
        {
            var identity = await _userManager.CreateUserIdentityAsync(registrationServiceResponse.Account.Id,
                registrationModel.Email, registrationModel.Email, null, registrationServiceResponse.Token, true);

            await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.RegistrationScheme, new ClaimsPrincipal(identity));
        }

        private Task<AccountsRegistrationResponseModel> CreateUserAsync(RegistrationModel registrationModel, string cid)
        {
            var model = _requestModelFactory.CreateForRegistrationService(
                registrationModel,
                HttpContext.GetIp(),
                HttpContext.GetUserAgent(),
                cid,
                HttpContext.GetReferer() ?? Request.GetUri().ToString(),
                Request.Cookies["sbjs_current"]
            );

            return _registrationServiceClient.RegistrationApi.RegisterAsync(model);
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
