﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Common.PasswordTools;
using Core;
using Core.Email;
using Core.Extensions;
using Core.Recaptcha;
using Core.VerificationCodes;
using Lykke.Common;
using Lykke.Common.Extensions;
using Lykke.Common.Log;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.ConfirmationCodes.Client.Models.Request;
using Lykke.Service.IpGeoLocation;
using Lykke.Service.Kyc.Abstractions.Domain.Documents.Types;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.OAuth.Models;
using Lykke.Service.Registration;
using Lykke.Service.Registration.Contract.Client.Enums;
using Lykke.Service.Registration.Contract.Client.Models;
using Lykke.Service.Session.Client;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using WebAuth.ActionHandlers;
using WebAuth.Managers;
using WebAuth.Models;
using WebAuth.Settings.ServiceSettings;

namespace WebAuth.Controllers
{
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
        private readonly IKycDocumentsServiceV2 _kycDocumentsService;
        private readonly ILog _log;
        private static readonly Dictionary<string, List<string>> CustomViewsDictionary = new Dictionary<string, List<string>>
        {
            { "raiffeisenkryptowallet" , new List<string>{"ios"}},
            { "modernmoney" , new List<string>{"ios"}}
        };
        private readonly IIpGeoLocationClient _geoLocationClient;
        private readonly IEnumerable<CountryItem> _countries;

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
            IKycDocumentsServiceV2 kycDocumentsService)
        {
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
            _kycDocumentsService = kycDocumentsService;
            var codes = new CountryPhoneCodes();
            _countries = codes.GetCountries();
        }

        [HttpGet("~/signin/{platform?}")]
        [HttpGet("~/register")]
        public IActionResult Login(string returnUrl = null, string platform = null, [FromQuery] string partnerId = null, [FromQuery] string code = null)
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


            try
            {
                var model = new LoginViewModel
                {
                    ReturnUrl = returnUrl,
                    Referer = HttpContext.GetReferer() ?? Request.GetUri().ToString(),
                    LoginRecaptchaKey = _securitySettings.RecaptchaKey,
                    RegisterRecaptchaKey = _securitySettings.RecaptchaKey,
                    PartnerId = partnerId,
                    AffiliateCode = code
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

        private static string PlatformViewName(string platform, string viewName,  string partnerId = null)
        {
            if (partnerId != null)
            {
                CustomViewsDictionary.TryGetValue(partnerId.ToLower(), out var customViews);
                if (customViews != null && customViews.Contains(platform)) return $"{viewName}.{partnerId}.{platform}";
            }

            switch (platform?.ToLower())
            {
                case "android":
                    return $"{viewName}.android";
                case "ios":
                    return $"{viewName}.ios";
                default:
                    return viewName;
            }
        }

        [HttpGet("~/signin/afterlogin/{platform?}")]
        public ActionResult Afterlogin(string platform = null, string returnUrl = null)
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
        public async Task<ActionResult> Signin(LoginViewModel model, string platform = null)
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
                    ModelState.AddModelError("", "Technical problems during authorization.");
                    return View(viewName, model);
                }

                if (authResult == null)
                {
                    ModelState.AddModelError("", "Technical problems during authorization.");
                    return View(viewName, model);
                }

                _log.Info(authResult.Status == AuthenticationStatus.Ok ? "Successful login" : "Unsuccessful login ", new { status = authResult.Status, requestModel.Ip, requestModel.UserAgent, clientId = authResult.Account?.Id});

                if (authResult.Status == AuthenticationStatus.Error)
                {
                    ModelState.AddModelError("", "The username or password you entered is incorrect");
                    return View(viewName, model);
                }

                if (authResult.Status == AuthenticationStatus.Blocked)
                {
                    return View(PlatformViewName(platform, "Blocked"));
                }

                if (authResult.Account.State == AccountState.Suspended)
                {
                    return View(PlatformViewName(platform, "Suspended"));
                }

                var identity = await _userManager.CreateUserIdentityAsync(authResult.Account.Id, authResult.Account.Email, model.Username, authResult.Account.PartnerId, authResult.Token, false);

                await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme, new ClaimsPrincipal(identity));

                return RedirectToAction("Afterlogin",
                    new RouteValueDictionary(new
                    {
                        platform = platform,
                        returnUrl = model.ReturnUrl
                    }));
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

            var code = await _verificationCodesService.AddCodeAsync(model.Email, model.Referer, model.ReturnUrl, model.Cid, traffic, model.AffiliateCode);
            var url = Url.Action("Signup", "Authentication", new { key = code.Key }, Request.Scheme);
            await _emailFacadeService.SendVerifyCode(model.Email, code.Code, url);

            return RedirectToAction("Signup", new { key = code.Key });
        }

        [HttpGet("~/signup/{key}")]
        public async Task<ActionResult> Signup(string key)
        {
            if (!key.IsValidPartitionOrRowKey())
                return RedirectToAction("Signin");

            var verificationCode = await _verificationCodesService.GetCodeAsync(key);

            if (verificationCode == null)
                return RedirectToAction("Signin");

            ViewBag.RecaptchaKey = _securitySettings.RecaptchaKey;

            return View(verificationCode);
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
                var accountExistsModel = await _clientAccountClient.ClientAccountInformation.IsTraderWithEmailExistsAsync(existingCode.Email, null);
                result.IsEmailTaken = accountExistsModel.IsClientAccountExisting;

                if (result.IsEmailTaken)
                    await _verificationCodesService.DeleteCodeAsync(existingCode.Key);
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
        public async Task<VerificationCodeResult> SendPhoneCode([FromBody] VerificationCodeRequest request)
        {
            var result = new VerificationCodeResult();

            var code = await _verificationCodesService.GetCodeAsync(request.Key);

            if (code == null)
                return result;

            var clientInfo = await _clientAccountClient.ClientAccountInformation
                .GetClientByPhoneAndPartnerIdAsync(request.Phone);

            if (clientInfo != null)
            {
                result.IsPhoneTaken = true;
                return result;
            }

            if (!code.SmsSent)
            {
                await _confirmationCodesClient.SendSmsConfirmationAsync(new SendSmsConfirmationRequest 
                { 
                    Phone = request.Phone,
                    Reason = "Lykke.Service.OAuth:/signup/sendPhoneCode",
                    OuterRequestId = HttpContext.TraceIdentifier
                });

                await _verificationCodesService.SetSmsSentAsync(request.Key);
            }

            return result;
        }

        [HttpPost("~/signup/verifyPhone")]
        [ValidateAntiForgeryToken]
        public async Task<VerificationCodeResult> VerifyPhone([FromBody] VerificationCodeRequest request)
        {
            var result = new VerificationCodeResult();

            if (request == null || !request.Key.IsValidPartitionOrRowKey())
                return result;

            var code = await _verificationCodesService.GetCodeAsync(request.Key);

            if (code == null)
                return result;

            var resCode = await _confirmationCodesClient.VerifySmsCodeAsync(new VerifySmsConfirmationRequest() { Code = request.Code, Phone = request.Phone });
            result.IsValid = resCode.IsValid;

            result.IsUkUser = await IsUkUser(request.CountryOfResidence);

            return result;
        }

        [HttpPost("~/signup/checkPassword")]
        [ValidateAntiForgeryToken]
        public bool CheckPassword([FromBody]string password)
        {
            return IsPasswordComplex(password);
        }

        [HttpPost("~/signup/checkAffiliateCode")]
        [ValidateAntiForgeryToken]
        public Task<bool> CheckAffiliateCode([FromBody]string code)
        {
            return _registrationClient.RegistrationApi.CheckAffilicateCodeAsync(code);
        }

        [HttpPost("~/signup/complete")]
        [ValidateAntiForgeryToken]
        public async Task<RegistrationResultModel> CompleteRegistration([FromBody]SignUpViewModel model)
        {
            string referer = null;
            var userIp = HttpContext.GetIp();
            var userAgent = HttpContext.GetUserAgent();
            var sanitizedEmail = model.Email?.SanitizeEmail();
            var sanitizedPhone = model.Phone?.SanitizePhone();

            _log.Info("Registration completion is being processed", context: new
            {
                Ip = userIp,
                UserAgent = userAgent,
                Email = sanitizedEmail,
                Phone = sanitizedPhone,
                FirstName = model.FirstName,
                LastName = model.LastName
            });

            var isAffiliateCodeCorrect = string.IsNullOrEmpty(model.AffiliateCode);

            if (!isAffiliateCodeCorrect)
            {
                isAffiliateCodeCorrect =
                    await _registrationClient.RegistrationApi.CheckAffilicateCodeAsync(model.AffiliateCode);
            }

            var registrationResult = new RegistrationResultModel
            {
                IsPasswordComplex = IsPasswordComplex(model.Password),
                IsAffiliateCodeCorrect = isAffiliateCodeCorrect
            };

            if (!registrationResult.IsValid)
            {
                _log.Info("Registration completion failed: insufficient password complexity or invalid affiliate code", context: new
                {
                    Ip = userIp,
                    UserAgent = userAgent,
                    Email = sanitizedEmail,
                    Phone = sanitizedPhone,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                });

                return registrationResult;
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .Where(item => item.ValidationState == ModelValidationState.Invalid)
                    .SelectMany(item => item.Errors);

                foreach (var error in errors)
                {
                    registrationResult.Errors.Add(error.ErrorMessage);
                }
            }

            if (!model.Email.IsValidEmailAndRowKey())
            {
                registrationResult.Errors.Add("Invalid email address");

                _log.Info("Registration completion failed: invalid email", context: new
                {
                    Ip = userIp,
                    UserAgent = userAgent,
                    Email = sanitizedEmail,
                    Phone = sanitizedPhone,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                });

                return registrationResult;
            }

            if (!string.IsNullOrEmpty(model.Referer))
            {
                try
                {
                    referer = new Uri(model.Referer).Host;
                }
                catch
                {
                    registrationResult.Errors.Add("Invalid referer url");

                    _log.Info("Registration completion failed: invalid referer url", context: new
                    {
                        Ip = userIp,
                        UserAgent = userAgent,
                        Email = sanitizedEmail,
                        Phone = sanitizedPhone,
                        FirstName = model.FirstName,
                        LastName = model.LastName
                    });

                    return registrationResult;
                }
            }

            var registrationRequest = new AccountRegistrationModel
            {
                Email = model.Email,
                Password = PasswordKeepingUtils.GetClientHashedPwd(model.Password),
                Hint = model.Hint,
                Ip = userIp,
                Changer = RecordChanger.Client,
                UserAgent = userAgent,
                Referer = referer,
                CreatedAt = DateTime.UtcNow,
                Cid = model.Cid,
                Traffic = model.Traffic,
                Ttl = GetSessionTtl(null),
                CountryFromPOA = model.CountryOfResidence,
                AffiliateCode = model.AffiliateCode,
                ContactPhone = model.Phone
            };

            var registrationResponse = await _registrationClient.RegistrationApi.RegisterAsync(registrationRequest);

            registrationResult.RegistrationResponse = registrationResponse;

            if (registrationResult.RegistrationResponse == null)
            {
                registrationResult.Errors.Add("Technical problems during registration.");

                _log.Info($"Registration completion failed: failed to invoke registration API. Empty response. Error: '{registrationResult.Errors?.ToJson()}'", context: new
                {
                    Ip = userIp,
                    UserAgent = userAgent,
                    Email = sanitizedEmail,
                    Phone = sanitizedPhone,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                });

                return registrationResult;
            }

            if (registrationResult.Errors.Any())
            {
                _log.Info($"Registration completion failed: failed to invoke registration API. Error: '{registrationResult.Errors?.ToJson()}'", context: new
                {
                    Ip = userIp,
                    UserAgent = userAgent,
                    Email = sanitizedEmail,
                    Phone = sanitizedPhone,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                });

                return registrationResult;
            }

            var clientId = registrationResult.RegistrationResponse.Account.Id;

            _log.Info("Registration completion: registration API invoked successfully", context: new
            {
                ClientId = clientId,
                Ip = userIp,
                UserAgent = userAgent,
                Email = sanitizedEmail,
                Phone = sanitizedPhone,
                FirstName = model.FirstName,
                LastName = model.LastName
            });

            await Task.WhenAll(
                _profileActionHandler.UpdatePersonalInformation(
                    registrationResponse.Account.Id,
                    model.FirstName,
                    model.LastName,
                    model.Phone),
                _verificationCodesService.DeleteCodeAsync(model.Key)
            );

            _log.Info("Registration completion: personal information and verification codes updated", context: new
            {
                ClientId = clientId,
                Ip = userIp,
                UserAgent = userAgent,
                Email = sanitizedEmail,
                Phone = sanitizedPhone,
                FirstName = model.FirstName,
                LastName = model.LastName
            });

            if (registrationResult.RegistrationResponse.Account.State != AccountState.Ok)
            {
                _log.Info($"Registration completion: invalid account state: {registrationResult.RegistrationResponse.Account.State}", context: new
                {
                    ClientId = clientId,
                    Ip = userIp,
                    UserAgent = userAgent,
                    Email = sanitizedEmail,
                    Phone = sanitizedPhone,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                });

                return registrationResult;
            }

            var identity = await _userManager.CreateUserIdentityAsync(
                registrationResponse.Account.Id,
                model.Email,
                model.Email,
                null,
                registrationResponse.Token,
                true);

            _log.Info("Registration completion: user identity has been created", context: new
            {
                ClientId = clientId,
                Ip = userIp,
                UserAgent = userAgent,
                Email = sanitizedEmail,
                Phone = sanitizedPhone,
                FirstName = model.FirstName,
                LastName = model.LastName
            });

            await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme,
                new ClaimsPrincipal(identity));

            if (model.UkUserQuestionnaire != null)
            {
                var ukUserQuestionnaireJson = JsonConvert.SerializeObject(model.UkUserQuestionnaire, Formatting.Indented);
                var ukUserQuestionnaireJsonBytes = Encoding.UTF8.GetBytes(ukUserQuestionnaireJson);
                    
                await _kycDocumentsService.UploadFileAsync(
                    registrationResult.RegistrationResponse.Account.Id,
                    OtherDocument.Name,
                    "json",
                    "UK questionnaire",
                    "UkQuestionnaire.json",
                    "application/json",
                    ukUserQuestionnaireJsonBytes,
                    "User");

                _log.Info("Registration completion: UK user questionnaire uploaded", context: new
                {
                    ClientId = clientId,
                    Ip = userIp,
                    UserAgent = userAgent,
                    Email = sanitizedEmail,
                    Phone = sanitizedPhone,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                });
            }

            _log.Info("Registration completion finished", context: new
            {
                ClientId = clientId,
                Ip = userIp,
                UserAgent = userAgent,
                Email = sanitizedEmail,
                Phone = sanitizedPhone,
                FirstName = model.FirstName,
                LastName = model.LastName
            });     

            return registrationResult;
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

        private async Task<bool> IsUkUser(string countryOfResidence)
        {
            // LM-3152: Users with a UK IP address and/or who specify the UK as a country of residence need to be redirected to the new webpage

            const string UnitedKindomIso3Code = "GBR";
                        
            if (countryOfResidence == UnitedKindomIso3Code)
            {
                return true;
            }
            else
            {
                var localityData = await _geoLocationClient.GetLocalityDataAsync(HttpContext.GetIp());

                if (localityData.Country == CountryManager.GetCountryNameByIso3(UnitedKindomIso3Code))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
