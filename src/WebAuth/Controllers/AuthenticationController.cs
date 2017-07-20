using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessService.Kyc;
using Common.Extensions;
using Common.Log;
using Common.PasswordTools;
using Core.Clients;
using Core.Kyc;
using Lykke.Service.Registration;
using Lykke.Service.Registration.Models;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Extensions;
using WebAuth.Managers;
using WebAuth.Models;

namespace WebAuth.Controllers
{
    public class AuthenticationController : BaseController
    {
        private readonly ILykkeRegistrationClient _registrationClient;
        private readonly IRegistrationConsumer[] _registrationConsumers;
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly IUserManager _userManager;
        private readonly ILog _log;

        public AuthenticationController(
            ILykkeRegistrationClient registrationClient,
            IEnumerable<IRegistrationConsumer> registrationConsumers,
            IClientAccountsRepository clientAccountsRepository,
            IUserManager userManager, 
            ILog log)
        {
            _registrationClient = registrationClient;
            _registrationConsumers = registrationConsumers.ToArray();
            _clientAccountsRepository = clientAccountsRepository;
            _userManager = userManager;
            _log = log;
        }

        [HttpGet("~/signin")]
        [HttpGet("~/register")]
        public ActionResult Login(string returnUrl = null)
        {
            string referer = this.GetReferer() ?? Request.GetUri().ToString();

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

            if (await _clientAccountsRepository.IsTraderWithEmailExistsAsync(registrationModel.Email))
            {
                ModelState.AddModelError("", $"Email {registrationModel.Email} is already in use.");
                return View("Login", model);
            }

            string userIp = this.GetIp();
            string referer = null;
            string userAgent = this.GetUserAgent();

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

            foreach (var registrationConsumer in _registrationConsumers)
                registrationConsumer.ConsumeRegistration(clientAccount, userIp, CultureInfo.CurrentCulture.Name);

            var identity = await _userManager.CreateUserIdentityAsync(clientAccount, registrationModel.Email);
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