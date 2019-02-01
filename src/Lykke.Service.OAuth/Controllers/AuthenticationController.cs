using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using Common;
using Common.Log;
using Common.PasswordTools;
using Core;
using Core.Email;
using Core.Extensions;
using Core.ExternalProvider;
using Core.ExternalProvider.Settings;
using Core.Recaptcha;
using Core.Registration;
using Core.Services;
using Core.VerificationCodes;
using IdentityModel;
using Lykke.Common;
using Lykke.Common.Extensions;
using Lykke.Common.Log;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.ConfirmationCodes.Client.Models.Request;
using Lykke.Service.IpGeoLocation;
using Lykke.Service.OAuth.Models;
using Lykke.Service.OAuth.Models.Registration;
using Lykke.Service.Registration;
using Lykke.Service.Registration.Contract.Client.Enums;
using Lykke.Service.Registration.Contract.Client.Models;
using Lykke.Service.Session.Client;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using WebAuth.ActionHandlers;
using WebAuth.Managers;
using WebAuth.Models;
using WebAuth.Settings.ServiceSettings;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace WebAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthenticationController : BaseController
    {
        private readonly IRegistrationServiceClient _registrationClient;
        private readonly IConfirmationCodesClient _confirmationCodesClient;
        private readonly IVerificationCodesService _verificationCodesService;
        private readonly IEmailFacadeService _emailFacadeService;
        private readonly ProfileActionHandler _profileActionHandler;
        private readonly IUserManager _userManager;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IRecaptchaService _recaptchaService;
        private readonly SecuritySettings _securitySettings;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly ILog _log;
        private static readonly Dictionary<string, List<string>> CustomViewsDictionary = new Dictionary<string, List<string>>
        {
            { "raiffeisenkryptowallet" , new List<string>{"ios"}},
            { "modernmoney" , new List<string>{"ios"}}
        };
        private readonly IIpGeoLocationClient _geoLocationClient;
        private readonly IEnumerable<CountryItem> _countries;
        private readonly IRegistrationRepository _registrationRepository;
        private readonly ISalesforceService _salesforceService;
        private readonly IExternalUserOperator _externalUserOperator;
        private readonly RedirectSettings _redirectSettings;
        private readonly ITokenService _tokenService;

        public AuthenticationController(
            IRegistrationServiceClient registrationClient,
            IVerificationCodesService verificationCodesService,
            IEmailFacadeService emailFacadeService,
            ProfileActionHandler profileActionHandler,
            IUserManager userManager,
            IClientAccountClient clientAccountClient,
            IRecaptchaService recaptchaService,
            SecuritySettings securitySettings,
            IConfirmationCodesClient confirmationCodesClient,
            IIpGeoLocationClient geoLocationClient,
            ILogFactory logFactory,
            IClientSessionsClient clientSessionsClient,
            IRegistrationRepository registrationRepository,
            ISalesforceService salesforceService,
            IRedirectSettingsAccessor redirectSettingsAccessor,
            IExternalUserOperator externalUserOperator, 
            ITokenService tokenService)
        {
            _registrationRepository = registrationRepository;
            _salesforceService = salesforceService;
            _externalUserOperator = externalUserOperator;
            _tokenService = tokenService;
            _redirectSettings = redirectSettingsAccessor.RedirectSettings;
            _registrationClient = registrationClient;
            _verificationCodesService = verificationCodesService;
            _emailFacadeService = emailFacadeService;
            _profileActionHandler = profileActionHandler;
            _userManager = userManager;
            _clientAccountClient = clientAccountClient;
            _recaptchaService = recaptchaService;
            _securitySettings = securitySettings;
            _confirmationCodesClient = confirmationCodesClient;
            _geoLocationClient = geoLocationClient;
            _log = logFactory.CreateLog(this);
            _clientSessionsClient = clientSessionsClient;
            _countries = new CountryPhoneCodes().GetCountries();
        }

        [HttpGet("~/signin/{platform?}")]
        [HttpGet("~/register")]
        public async Task<IActionResult> Login(string returnUrl = null, string platform = null, [FromQuery] string partnerId = null)
        {

            // Temporally disabled by LWDEV-9406. Enable after the mobile client has been completed.
            //            if (User.Identities.Any(identity => identity.IsAuthenticated))
            //            {
            //                var sessionId = User.FindFirst(OpenIdConnectConstantsExt.Claims.SessionId)?.Value;
            //                if (sessionId != null)
            //                {
            //                    return RedirectToAction("Afterlogin", new { returnUrl, platform });
            //                }
            //            }
            var afterIroncladLoginUrl = await _externalUserOperator.GetIroncladRequestAsync();

            if (string.IsNullOrWhiteSpace(afterIroncladLoginUrl))
            {
                var relativeUrl = UriHelper.BuildRelative(HttpContext.Request.PathBase, HttpContext.Request.Path,
                    HttpContext.Request.QueryString);

                await _externalUserOperator.SaveLykkeSignInContextAsync(relativeUrl);

                var afterLykkeLoginReturnUrl = Url.Action("Afterlogin",
                    new RouteValueDictionary(new
                    {
                        platform = platform
                    }));

                var properties = new AuthenticationProperties
                {
                    RedirectUri = Url.Action("LykkeLoginCallback", "External")
                };

                var clientRedirectUri = await _externalUserOperator.GetClientRedirectUriAsync();

                var externalLoginRedirectUrl = clientRedirectUri ?? afterLykkeLoginReturnUrl;

                //TODO:@gafanasiev May be move upper, before if for ironclad.
                if (User.Identities.Any(identity => identity.IsAuthenticated))
                {
                    var redirectUrl = string.IsNullOrWhiteSpace(returnUrl) ? afterLykkeLoginReturnUrl : returnUrl;

                    /* NOTE: We should not use here clientRedirectUri
                     * because if user is already logged in
                     * it would be handled in /authorize */
                    return LocalRedirect(redirectUrl);
                }

                properties.SetProperty(OpenIdConnectConstantsExt.AuthenticationProperties.ExternalLoginRedirectUrl, externalLoginRedirectUrl);

                // Set idp for ironclad, to use lykke as external provider.
                properties.SetProperty(OpenIdConnectConstantsExt.AuthenticationProperties.AcrValues, _redirectSettings.OldLykkeSignInIroncladAuthAcrValues);

                return Challenge(properties, OpenIdConnectConstantsExt.Auth.IroncladAuthenticationScheme);
            }

            try
            {
                var lykkeToken = User?.GetClaim(OpenIdConnectConstantsExt.Claims.SessionId);

                //TODO:refactor
                var logoutUrl = "http://localhost:5005/connect/endsession";

                if (!string.IsNullOrWhiteSpace(lykkeToken))
                {
                    var tokens = await _tokenService.GetIroncladTokens(lykkeToken);
                    var idToken = tokens?.IdToken;
                    if (!string.IsNullOrWhiteSpace(idToken))
                        QueryHelpers.AddQueryString(logoutUrl, OidcConstants.EndSessionRequest.IdTokenHint, idToken);
                }

                var model = new LoginViewModel
                {
                    ReturnUrl = returnUrl,
                    Referer = HttpContext.GetReferer() ?? Request.GetUri().ToString(),
                    LoginRecaptchaKey = _securitySettings.RecaptchaKey,
                    RegisterRecaptchaKey = _securitySettings.RecaptchaKey,
                    PartnerId = partnerId,
                    IroncladLogoutUrl = logoutUrl
                };

                var viewName = PlatformToViewName(platform, partnerId);

                return View(viewName, model);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Content(ex.Message);
            }
        }

        [HttpGet("~/signin/afterlogin/{platform?}")]
        public async Task<ActionResult> Afterlogin(string platform = null, string returnUrl = null)
        {
            switch (platform?.ToLower())
            {
                case "android":
                    return RedirectToAction("GetLykkeWalletTokenMobile", "Userinfo");
                case "ios":
                    return View("AfterLogin.ios");
                default:
                    return RedirectToLocal(returnUrl);
            }
        }

        [HttpPost("~/signin/{platform?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signin(LoginViewModel model, string platform = null)
        {
            if (model == null)
            {
                return BadRequest();
            }

            model.LoginRecaptchaKey = _securitySettings.RecaptchaKey;
            model.RegisterRecaptchaKey = _securitySettings.RecaptchaKey;

            var viewName = PlatformToViewName(platform, model.PartnerId);

            if (model.IsLogin.HasValue && model.IsLogin.Value)
            {
                if (!model.Username.IsValidEmailAndRowKey())
                    ModelState.AddModelError(nameof(model.Username), "Please enter a valid email address");

                if (!await _recaptchaService.Validate())
                    ModelState.AddModelError(nameof(model.LoginRecaptchaKey), "Captcha validation failed");

                if (!ModelState.IsValid)
                    return View(viewName, model);

                var registrationLogin = await HandleAuthenticationForUnfinishedRegistrationAsync(model, platform, viewName);

                if (registrationLogin != null)
                    return registrationLogin;

                var afterIroncladLoginUrl = await _externalUserOperator.GetIroncladRequestAsync();

                if (!string.IsNullOrWhiteSpace(afterIroncladLoginUrl))
                    return await HandleAuthenticationThroughIroncladAsync(model, afterIroncladLoginUrl, viewName);
               
                return await HandleAuthenticationAsync(model, platform, viewName);
            }

            ModelState.ClearValidationState(nameof(model.Username));
            ModelState.ClearValidationState(nameof(model.Password));

            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), $"{nameof(model.Email)} is required and can't be empty");

                return View(viewName, model);
            }

            if (!model.Email.IsValidEmailAndRowKey())
            {
                ModelState.AddModelError(nameof(model.Email), "Please enter a valid email address");
                return View(viewName, model);
            }

            if (!await _recaptchaService.Validate())
            {
                ModelState.AddModelError(nameof(model.RegisterRecaptchaKey), "Captcha validation failed");
                return View(viewName, model);
            }

            var traffic = Request.Cookies["sbjs_current"];

            var code = await _verificationCodesService.AddCodeAsync(model.Email, model.Referer, model.ReturnUrl, model.Cid, traffic);
            var url = Url.Action("Signup", "Authentication", new { key = code.Key }, Request.Scheme);
            await _emailFacadeService.SendVerifyCode(model.Email, code.Code, url);

            return RedirectToAction("Signup", new { key = code.Key });
        }

        [HttpGet("~/signup/{key}")]
        public async Task<ActionResult> Signup(string key)
        {
            if (!key.IsValidPartitionOrRowKey())
                return RedirectToAction("Signin");

            var code = await _verificationCodesService.GetCodeAsync(key);

            if (code == null)
                return RedirectToAction("Signin");

            ViewBag.RecaptchaKey = _securitySettings.RecaptchaKey;

            return View(code);
        }

        [HttpPost("~/signup/verifyEmail")]
        [ValidateAntiForgeryToken]
        public async Task<VerificationCodeResult> VerifyEmail([FromBody] VerificationCodeRequest request)
        {
            var result = new VerificationCodeResult();

            if (request == null || !request.Key.IsValidPartitionOrRowKey())
                return result;

            var existingCode = await _verificationCodesService.GetCodeAsync(request.Key);

            result.IsCodeExpired = existingCode == null;

            if (existingCode != null && existingCode.Code == request.Code)
            {
                result.Code = existingCode;
                AccountExistsModel accountExistsModel = await _clientAccountClient.IsTraderWithEmailExistsAsync(existingCode.Email, null);
                result.IsEmailTaken = accountExistsModel.IsClientAccountExisting;

                if (result.IsEmailTaken)
                {
                    await _verificationCodesService.DeleteCodeAsync(existingCode.Key);
                }
                else
                {
                    _salesforceService.CreateContact(existingCode.Email);
                }
            }

            return result;
        }

        [HttpPost("~/signup/resendCode")]
        [ValidateAntiForgeryToken]
        public async Task<ResendCodeResult> ResendCode([FromBody] ResendCodeRequest request)
        {
            var result = new ResendCodeResult();

            if (!request.Key.IsValidPartitionOrRowKey())
                return result;

            if (string.IsNullOrEmpty(request.Captcha) || !await _recaptchaService.Validate(request.Captcha))
                return result;

            var code = await _verificationCodesService.GetCodeAsync(request.Key);

            if (code == null)
                return ResendCodeResult.Expired;

            if (code.ResendCount > 2)
                return result;

            code = await _verificationCodesService.UpdateCodeAsync(request.Key);

            if (code == null)
                return ResendCodeResult.Expired;

            var url = Url.Action("Signup", "Authentication", new { key = code.Key }, Request.Scheme);
            await _emailFacadeService.SendVerifyCode(code.Email, code.Code, url);
            result.Result = true;
            return result;
        }
        [HttpPost("~/signup/countrieslist")]
        [ValidateAntiForgeryToken]
        public async Task<CountryModel> CountriesList()
        {
            var localityData = await _geoLocationClient.GetLocalityDataAsync(HttpContext.GetIp());
            var model = new CountryModel();
            var countries = _countries
                .Select(o => new CountryViewModel
                {
                    Id = o.Id,
                    Title = o.Name,
                    Prefix = o.Prefix,
                    Selected = localityData?.Country != null && localityData.Country == o.Name
                })
                .ToList();
            model.Data = countries;
            return model;
        }
        [HttpPost("~/signup/sendPhoneCode")]
        [ValidateAntiForgeryToken]
        public async Task SendPhoneCode([FromBody] VerificationCodeRequest request)
        {
            await _confirmationCodesClient.SendSmsConfirmationAsync(new SendSmsConfirmationRequest() { Phone = request.Code });
        }
        [HttpPost("~/signup/verifyPhone")]
        [ValidateAntiForgeryToken]
        public async Task<VerificationCodeResult> VerifyPhone([FromBody] VerificationCodeRequest request)
        {
            var result = new VerificationCodeResult();

            if (request == null || !request.Key.IsValidPartitionOrRowKey())
                return result;

            var resCode = await _confirmationCodesClient.VerifySmsCodeAsync(new VerifySmsConfirmationRequest() { Code = request.Code, Phone = request.Phone });
            result.IsValid = resCode.IsValid;
            return result;
        }
        [HttpPost("~/signup/checkPassword")]
        [ValidateAntiForgeryToken]
        public bool CheckPassword([FromBody]string password)
        {
            return IsPasswordComplex(password);
        }

        [HttpPost("~/signup/complete")]
        [ValidateAntiForgeryToken]
        public async Task<RegistrationResultModel> CompleteRegistration([FromBody]SignUpViewModel model)
        {
            var regResult = new RegistrationResultModel
            {
                IsPasswordComplex = IsPasswordComplex(model.Password)
            };

            if (!regResult.IsPasswordComplex)
                return regResult;

            if (ModelState.IsValid)
            {
                if (!model.Email.IsValidEmailAndRowKey())
                {
                    regResult.Errors.Add("Invalid email address");
                    return regResult;
                }

                string userIp = HttpContext.GetIp();
                string referer = null;
                string userAgent = HttpContext.GetUserAgent();

                if (!string.IsNullOrEmpty(model.Referer))
                {
                    try
                    {
                        referer = new Uri(model.Referer).Host;
                    }
                    catch
                    {
                        regResult.Errors.Add("Invalid referer url");
                        return regResult;
                    }
                }

                var result = await _registrationClient.RegistrationApi.RegisterAsync(new AccountRegistrationModel
                {
                    Email = model.Email,
                    Password = PasswordKeepingUtils.GetClientHashedPwd(model.Password),
                    Hint = model.Hint,
                    Ip = userIp,
                    Changer = RecordChanger.Client,
                    UserAgent = userAgent,
                    Referer = referer,
                    CreatedAt = DateTime.UtcNow,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    FullName = $"{model.FirstName} {model.LastName}",
                    ContactPhone = model.Phone,
                    Cid = model.Cid,
                    Traffic = model.Traffic,
                    Ttl = GetSessionTtl(null),
                    CountryFromPOA = model.CountryOfResidence
                });

                regResult.RegistrationResponse = result;

                if (regResult.RegistrationResponse == null)
                {
                    regResult.Errors.Add("Technical problems during registration.");
                    return regResult;
                }

                await _profileActionHandler.UpdatePersonalInformation(result.Account.Id, model.FirstName, model.LastName, model.Phone);

                var identity = await _userManager.CreateUserIdentityAsync(result.Account.Id, model.Email, model.Email, null, result.Token, true);

                await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme, new ClaimsPrincipal(identity));

                await _verificationCodesService.DeleteCodeAsync(model.Key);
            }
            else
            {
                var errors = ModelState.Values
                    .Where(item => item.ValidationState == ModelValidationState.Invalid)
                    .SelectMany(item => item.Errors);

                foreach (var error in errors)
                {
                    regResult.Errors.Add(error.ErrorMessage);
                }
            }

            return regResult;
        }

        [HttpGet("~/signout")]
        [HttpPost("~/signout")]
        public async Task<IActionResult> SignOut()
        {
            var sessionId = User.FindFirst(OpenIdConnectConstantsExt.Claims.SessionId)?.Value;
            if (sessionId != null)
            {
                await _clientSessionsClient.DeleteSessionIfExistsAsync(sessionId);
            }
            return SignOut(OpenIdConnectConstantsExt.Auth.DefaultScheme);
        }

        [HttpGet("~/afterregistrationlogin")]
        public IActionResult AfterRegistrationLogin(AfterRegistrationLoginRequest request)
        {
            return Json(new AfterRegistrationLoginResponse
            {
                RegistrationId = request.RegistrationId
            });
        }

        private static bool IsPasswordComplex(string password)
        {
            return password.IsPasswordComplex(useSpecialChars: false);
        }

        private static TimeSpan? GetSessionTtl(string platform)
        {
            switch (platform?.ToLower())
            {
                case "android":
                case "ios":
                    return TimeSpan.FromDays(30);

                default:
                    return null;
            }
        }

        private async Task<IActionResult> HandleAuthenticationAsync(LoginViewModel model, string platform,
            string viewName)
        {
            AuthenticateResponseModel authResult;
            var requestModel = new AuthenticateModel
            {
                Email = model.Username,
                Password = model.Password,
                Ip = HttpContext.GetIp(),
                UserAgent = HttpContext.GetUserAgent(),
                PartnerId = model.PartnerId,
                Ttl = GetSessionTtl(platform)
            };
            try
            {
                authResult = await _registrationClient.LoginApi.AuthenticateAsync(requestModel);
            }
            catch (Exception ex)
            {
                _log.Error(nameof(AuthenticationController), ex, requestModel.Sanitize().ToJson());
                ModelState.AddModelError("", "Technical problems during authentication.");
                return View(viewName, model);
            }

            if (authResult == null)
            {
                ModelState.AddModelError("", "Technical problems during authentication.");
                return View(viewName, model);
            }

            if (authResult.Status == AuthenticationStatus.Error) return IncorrectUsernameOrPassword(model, viewName);

            var identity = await _userManager.CreateUserIdentityAsync(authResult.Account.Id, authResult.Account.Email,
                model.Username, authResult.Account.PartnerId, authResult.Token, false);

            await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Afterlogin",
                new RouteValueDictionary(new
                {
                    platform,
                    returnUrl = model.ReturnUrl
                }));
        }

        private async Task<IActionResult> HandleAuthenticationForUnfinishedRegistrationAsync(LoginViewModel model, string platform,
            string viewName)
        {
            var userModel = await _registrationRepository.GetByEmailAsync(model.Username);

            if (userModel == null)
                return null;

            if (!userModel.CheckPassword(model.Password))
                return IncorrectUsernameOrPassword(model, viewName);

            await _externalUserOperator.ClearIroncladRequestAsync();

            if (platform == "android" || platform == "ios")
                return RedirectToAction("AfterRegistrationLogin", new AfterRegistrationLoginRequest
                {
                    RegistrationId = userModel.RegistrationId
                });

            return RedirectToAction("Registration", "Spa",
                new
                {
                    registrationId = userModel.RegistrationId
                });
        }

        private async Task<IActionResult> HandleAuthenticationThroughIroncladAsync(LoginViewModel model,
            string afterIroncladLoginUrl, string viewName)
        {
            var lykkeUserId =
                await _externalUserOperator.AuthenticateLykkeUserAsync(model.Username, model.Password, model.PartnerId);

            if (lykkeUserId == null) return IncorrectUsernameOrPassword(model, viewName);

            await _externalUserOperator.SaveTempLykkeUserIdAsync(lykkeUserId);

            await _externalUserOperator.ClearIroncladRequestAsync();

            return LocalRedirect(afterIroncladLoginUrl);
        }

        private static string PlatformToViewName(string platform, string partnerId)
        {
            if (partnerId != null)
            {
                CustomViewsDictionary.TryGetValue(partnerId.ToLower(), out var customViews);
                if (customViews != null && customViews.Contains(platform)) return $"Login.{partnerId}.{platform}";
            }

            switch (platform?.ToLower())
            {
                case "android":
                    return "Login.android";
                case "ios":
                    return "Login.ios";
                default:
                    return "Login";
            }
        }

        private IActionResult IncorrectUsernameOrPassword(LoginViewModel model, string viewName)
        {
            ModelState.AddModelError("", "The username or password you entered is incorrect");
            return View(viewName, model);
        }
    }
}
