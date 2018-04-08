using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Server;
using Common;
using Common.Log;
using Common.PasswordTools;
using Core;
using Core.Email;
using Core.Extensions;
using Lykke.Common.Extensions;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.Registration;
using Lykke.Service.Registration.Models;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebAuth.ActionHandlers;
using WebAuth.Managers;
using WebAuth.Models;

namespace WebAuth.Controllers
{
    public class AuthenticationController : BaseController
    {
        private readonly ILykkeRegistrationClient _registrationClient;
        private readonly IVerificationCodesRepository _verificationCodesRepository;
        private readonly IEmailFacadeService _emailFacadeService;
        private readonly ProfileActionHandler _profileActionHandler;
        private readonly IUserManager _userManager;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly ILog _log;

        public AuthenticationController(
            ILykkeRegistrationClient registrationClient,
            IVerificationCodesRepository verificationCodesRepository,
            IEmailFacadeService emailFacadeService,
            ProfileActionHandler profileActionHandler,
            IUserManager userManager,
            IClientAccountClient clientAccountClient,
            ILog log)
        {
            _registrationClient = registrationClient;
            _verificationCodesRepository = verificationCodesRepository;
            _emailFacadeService = emailFacadeService;
            _profileActionHandler = profileActionHandler;
            _userManager = userManager;
            _clientAccountClient = clientAccountClient;
            _log = log;
        }

        [HttpGet("~/signin")]
        [HttpGet("~/register")]
        public async Task<ActionResult> Login(string returnUrl = null)
        {
            try
            {
                string referer = HttpContext.GetReferer() ?? Request.GetUri().ToString();
                return View("Login", new LoginViewModel(returnUrl, referer));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(ex.Source, "Signin", null, ex);
                return Content(ex.Message);
            }
        }

        [HttpPost("~/signin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Signin(LoginViewModel model)
        {
            if (model.IsLogin)
            {
                if (!model.Username.IsValidEmailAndRowKey())
                    ModelState.AddModelError(nameof(model.Username), "Please enter a valid email address");

                if (!ModelState.IsValid)
                    return View("Login", model);

                AuthResponse authResult = await _registrationClient.AuthorizeAsync(new AuthModel
                {
                    Email = model.Username,
                    Password = model.Password,
                    Ip = HttpContext.GetIp(),
                    UserAgent = HttpContext.GetUserAgent()
                });

                if (authResult == null)
                {
                    ModelState.AddModelError("", "Technical problems during authorization.");
                    return View("Login", model);
                }

                if (authResult.Status == AuthenticationStatus.Error)
                {
                    ModelState.AddModelError("", "The username or password you entered is incorrect");
                    return View("Login", model);
                }

                var identity = await _userManager.CreateUserIdentityAsync(authResult.Account.Id,
                    authResult.Account.Email, model.Username, false);

                await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme, new ClaimsPrincipal(identity));

                return RedirectToLocal(model.ReturnUrl);
            }

            ModelState.ClearValidationState(nameof(model.Username));
            ModelState.ClearValidationState(nameof(model.Password));

            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), $"{nameof(model.Email)} is required and can't be empty");
                
                return View("Login", model);
            }

            if (!model.Email.IsValidEmailAndRowKey())
            {
                ModelState.AddModelError(nameof(model.Email), "Please enter a valid email address");
                return View("Login", model);
            }

            var code = await _verificationCodesRepository.AddCodeAsync(model.Email, model.Referer, model.ReturnUrl);
            var url = Url.Action("Signup", "Authentication", new {key = code.Key}, Request.Scheme);
            await _emailFacadeService.SendVerifyCode(model.Email, code.Code, url);

            return RedirectToAction("Signup", new {key = code.Key});
        }

        [HttpGet("~/signup/{key}")]
        public async Task<ActionResult> Signup(string key)
        {
            if (!key.IsValidPartitionOrRowKey())
                return RedirectToAction("Signin");

            var code = await _verificationCodesRepository.GetCodeAsync(key);

            if (code == null)
                return RedirectToAction("Signin");

            return View(code);
        }

        [HttpPost("~/signup/verifyEmail")]  
        [ValidateAntiForgeryToken]
        public async Task<VerificationCodeResult> VerifyEmail([FromBody] VerificationCodeRequest request)
        {
            var result = new VerificationCodeResult();

            if (request == null || !request.Key.IsValidPartitionOrRowKey())
                return result;

            var existingCode = await _verificationCodesRepository.GetCodeAsync(request.Key);

            if (existingCode != null && existingCode.Code == request.Code)
            {
                result.Code = existingCode;
                AccountExistsModel accountExistsModel = await _clientAccountClient.IsTraderWithEmailExistsAsync(existingCode.Email, null);
                result.IsEmailTaken = accountExistsModel.IsClientAccountExisting;

                if (result.IsEmailTaken)
                    await _verificationCodesRepository.DeleteCodesAsync(existingCode.Email);
            }

            return result;
        }

        [HttpPost("~/signup/resendCode")]  
        [ValidateAntiForgeryToken]
        public async Task ResendCode([FromBody]string key)
        {
            if (!key.IsValidPartitionOrRowKey())
                return;

            var code = await _verificationCodesRepository.GetCodeAsync(key);

            if (code != null && code.ResendCount < 2)
            {
                code = await _verificationCodesRepository.UpdateCodeAsync(key);
                var url = Url.Action("Signup", "Authentication", new { key = code.Key }, Request.Scheme);
                await _emailFacadeService.SendVerifyCode(code.Email, code.Code, url);
            }
        }
        
        [HttpPost("~/signup/checkPassword")]  
        [ValidateAntiForgeryToken]
        public bool CheckPassword([FromBody]string passowrd)
        {
            return passowrd.IsPasswordComplex();
        }

        [HttpPost("~/signup/complete")]
        [ValidateAntiForgeryToken]
        public async Task<RegistrationResultModel> CompleteRegistration([FromBody]SignUpViewModel model)
        {
            var regResult = new RegistrationResultModel();

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
                    
                RegistrationResponse result = await _registrationClient.RegisterAsync(new RegistrationModel
                {
                    Email = model.Email,
                    Password = PasswordKeepingUtils.GetClientHashedPwd(model.Password),
                    Ip = userIp,
                    Changer = RecordChanger.Client,
                    UserAgent = userAgent,
                    Referer = referer
                });

                regResult.RegistrationResponse = result;

                if (regResult.RegistrationResponse == null)
                {
                    regResult.Errors.Add("Technical problems during registration.");
                    return regResult;
                }

                var identity = await _userManager.CreateUserIdentityAsync(result.Account.Id, result.Account.Email, model.Email, true);

                await HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme, new ClaimsPrincipal(identity));

                await _profileActionHandler.UpdatePersonalInformation(result.Account.Id, model.FirstName, model.LastName);
                await _verificationCodesRepository.DeleteCodesAsync(model.Email);
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
        public async Task<ActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme);
            await HttpContext.SignOutAsync(OpenIdConnectServerDefaults.AuthenticationScheme);
            return SignOut(OpenIdConnectConstantsExt.Auth.DefaultScheme, OpenIdConnectServerDefaults.AuthenticationScheme);
        }
    }
}
