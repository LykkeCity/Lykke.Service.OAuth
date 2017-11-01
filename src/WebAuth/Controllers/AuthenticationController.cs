using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessService.Kyc;
using Common.Extensions;
using Common.Log;
using Common.PasswordTools;
using Core.Clients;
using Lykke.Service.Registration;
using Lykke.Service.Registration.Models;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Extensions;
using WebAuth.Managers;
using WebAuth.Models;

namespace WebAuth.Controllers
{
    public class AuthenticationController : BaseController
    {
        private readonly ILykkeRegistrationClient _registrationClient;
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly IUserManager _userManager;
        private readonly ILog _log;

        public AuthenticationController(
            ILykkeRegistrationClient registrationClient,
            IClientAccountsRepository clientAccountsRepository,
            IUserManager userManager, 
            ILog log)
        {
            _registrationClient = registrationClient;
            _clientAccountsRepository = clientAccountsRepository;
            _userManager = userManager;
            _log = log;
        }

        [HttpGet("~/signin")]
        [HttpGet("~/register")]
        public ActionResult Login(string returnUrl = null)
        {
            string referer = HttpContext.GetReferer() ?? Request.GetUri().ToString();

            try
            {
                return View("Login", new LoginViewModel(returnUrl, referer));
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

            AuthResponse authResult = await _registrationClient.AuthorizeAsync(new AuthModel
            {
                Email = loginModel.Username,
                Password = loginModel.Password,
                Ip = HttpContext.GetIp(),
                UserAgent = HttpContext.GetUserAgent()
            });


            if (authResult.Status == AuthenticationStatus.Error)
            {
                ModelState.AddModelError("Username", " ");
                ModelState.AddModelError("Password", "Invalid user");
                return View("Login", model);
            }

            var identity = await _userManager.CreateUserIdentityAsync(authResult.Account.Id, authResult.Account.Email, loginModel.Username, false);

            await HttpContext.Authentication.SignInAsync("ServerCookie", new ClaimsPrincipal(identity));

            return RedirectToLocal(loginModel.ReturnUrl);
        }

        [HttpPost("~/register")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegistrationViewModel registrationModel)
        {
            var model = new LoginViewModel(new SigninViewModel(registrationModel.ReturnUrl), registrationModel);

            if (!ModelState.IsValid)
                return View("Login", model);

            if (await _clientAccountsRepository.IsTraderWithEmailExistsAsync(registrationModel.Email))
            {
                ModelState.AddModelError("", $"Email {registrationModel.Email} is already in use.");
                return View("Login", model);
            }

            string userIp = HttpContext.GetIp();
            string referer = null;
            string userAgent = HttpContext.GetUserAgent();

            if (!string.IsNullOrEmpty(registrationModel.Referer))
            {
                referer = new Uri(registrationModel.Referer).Host;
            }

            RegistrationResponse result = await _registrationClient.RegisterAsync(new RegistrationModel
            {
                Email = registrationModel.Email,
                Password = PasswordKeepingUtils.GetClientHashedPwd(registrationModel.RegistrationPassword),
                Ip = userIp,
                Changer = RecordChanger.Client,
                UserAgent = userAgent,
                Referer = referer
            });

            if (result == null)
            {
                ModelState.AddModelError("", "Technical problems during registration.");
                return View("Login", model);
            }

            var clientAccount = new Core.Clients.ClientAccount
            {
                Id = result.Account.Id,
                Email = result.Account.Email,
                Registered = result.Account.Registered,
                NotificationsId = result.Account.NotificationsId,
                Phone = result.Account.Phone
            };

            var identity = await _userManager.CreateUserIdentityAsync(clientAccount.Id, clientAccount.Email, registrationModel.Email, true);

            await HttpContext.Authentication.SignInAsync("ServerCookie", new ClaimsPrincipal(identity));

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