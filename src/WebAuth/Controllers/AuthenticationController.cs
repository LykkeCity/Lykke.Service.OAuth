using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessService.Kyc;
using Common.Extenstions;
using Common.Log;
using Common.PasswordKeeping;
using Core.Clients;
using Core.Kyc;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Managers;
using WebAuth.Models;

namespace WebAuth.Controllers
{
    public class AuthenticationController : BaseController
    {
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly ISrvKycManager _srvKycManager;
        private readonly IUserManager _userManager;
        private readonly ILog _log;

        public AuthenticationController(IClientAccountsRepository clientAccountsRepository,
            IUserManager userManager, ISrvKycManager srvKycManager, ILog log)
        {
            _clientAccountsRepository = clientAccountsRepository;
            _userManager = userManager;
            _srvKycManager = srvKycManager;
            _log = log;
        }

        [HttpGet("~/signin")]
        [HttpGet("~/register")]
        public ActionResult Login(string returnUrl = null)
        {
            try
            {
                return View("Login", new LoginViewModel(returnUrl));
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(ex.Source, "Signin", null, ex).RunSync();
                return Content(ex.Message);
            }
        }

        [HttpPost("~/signin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Signin(SigninViewModel loginModel)
        {
            var model = new LoginViewModel(loginModel, new RegistrationViewModel(loginModel.ReturnUrl));
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            var clientAccount =
                await _clientAccountsRepository.AuthenticateAsync(loginModel.Username, loginModel.Password) ??
                //ToDo: to remove when migrated to hashes
                await
                    _clientAccountsRepository.AuthenticateAsync(loginModel.Username,
                        PasswordKeepingUtils.GetClientHashedPwd(loginModel.Password));

            if (clientAccount == null)
            {
                ModelState.AddModelError("Username", " ");
                ModelState.AddModelError("Password", "Invalid user");
                return View("Login", model);
            }

            var identity = await _userManager.CreateUserIdentityAsync(clientAccount, loginModel.Username);
            await
                HttpContext.Authentication.SignInAsync("ServerCookie", new ClaimsPrincipal(identity),
                    new AuthenticationProperties());

            return RedirectToLocal(loginModel.ReturnUrl);
        }

        [HttpPost("~/register")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegistrationViewModel registrationModel)
        {
            var model = new LoginViewModel(new SigninViewModel(registrationModel.ReturnUrl), registrationModel);
            if (!ModelState.IsValid)
                return View("Login", model);

            var userIp = HttpContext.Connection.RemoteIpAddress.ToString();

            if (await _clientAccountsRepository.IsTraderWithEmailExistsAsync(registrationModel.Email))
                return View("Login", model);

            var user =
                await
                    _srvKycManager.RegisterClientAsync(registrationModel.Email, string.Empty, string.Empty, string.Empty,
                        registrationModel.RegistrationPassword, string.Empty, string.Empty, userIp, RecordChanger.Client,
                        CultureInfo.CurrentCulture.Name);

            var identity = await _userManager.CreateUserIdentityAsync(user, registrationModel.Email);
            await
                HttpContext.Authentication.SignInAsync("ServerCookie", new ClaimsPrincipal(identity),
                    new AuthenticationProperties());

            return RedirectToAction("PersonalInformation", "Profile", new {returnUrl = registrationModel.ReturnUrl});
        }

        [HttpGet("~/signout")]
        [HttpPost("~/signout")]
        public ActionResult SignOut()
        {
            return SignOut("ServerCookie");
        }
    }
}