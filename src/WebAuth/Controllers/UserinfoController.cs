using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using Core.Application;
using Core.Bitcoin;
using Core.Clients;
using Core.Kyc;
using Lykke.Service.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Models;

namespace WebAuth.Controllers
{
    public class UserinfoController : Controller
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IKycRepository _kycRepository;
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly IClientsSessionsRepository _clientSessionsClient;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;

        public UserinfoController(
            IApplicationRepository applicationRepository, 
            IKycRepository kycRepository, 
            IClientAccountsRepository clientAccountsRepository,
            IClientsSessionsRepository clientSessionsClient,
            IWalletCredentialsRepository walletCredentialsRepository)
        {
            _applicationRepository = applicationRepository;
            _kycRepository = kycRepository;
            _clientAccountsRepository = clientAccountsRepository;
            _clientSessionsClient = clientSessionsClient;
            _walletCredentialsRepository = walletCredentialsRepository;
        }

        [Authorize(ActiveAuthenticationSchemes = OpenIdConnectServerDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo")]
        public IActionResult GetUserInfo()
        {
            var userInfo = new UserInfoViewModel
            {
                Email = User.GetClaim(OpenIdConnectConstants.Claims.Email),
                FirstName = User.GetClaim(OpenIdConnectConstants.Claims.GivenName),
                LastName = User.GetClaim(OpenIdConnectConstants.Claims.FamilyName)
            };
            return Json(userInfo);
        }

        [HttpGet("~/getkycstatus")]
        public async Task<IActionResult> GetKycStatus(string email)
        {
            var applicationId = HttpContext.Request.Headers["application_id"].ToString();
            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null) return Json("Application Id Incorrect!");

            var client = await _clientAccountsRepository.GetByEmailAsync(email);

            var kycStatus = await _kycRepository.GetKycStatusAsync(client.Id);
            return Json(kycStatus.ToString());
        }

        [HttpGet("~/getidbyemail")]
        public async Task<IActionResult> GetIdByEmail(string email)
        {
            var applicationId = HttpContext.Request.Headers["application_id"].ToString();
            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null) return Json("Application Id Incorrect!");

            var client = await _clientAccountsRepository.GetByEmailAsync(email);
            return Json(client.Id);
        }

        [HttpGet("~/getemailbyid")]
        public async Task<IActionResult> GetEmailById(string id)
        {
            var applicationId = HttpContext.Request.Headers["application_id"].ToString();
            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null) return Json("Application Id Incorrect!");

            var client = await _clientAccountsRepository.GetByIdAsync(id);
            return Json(client.Email);
        }

        [HttpGet("~/getlykkewallettoken")]
        public async Task<IActionResult> GetLykkewalletToken()
        {
            var applicationId = HttpContext.Request.Headers["application_id"].ToString();
            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null) return Json("Application Id Incorrect!");

            var clientId = User.GetClaim(ClaimTypes.NameIdentifier);
            string token = string.Empty;

            if (clientId != null)
            {
                var clientAccount = await _clientAccountsRepository.GetByIdAsync(clientId);

                if (clientAccount == null)
                {
                    return Json(new { Token = token });
                }
                
                var authResult = await _clientSessionsClient.Authenticate(clientAccount.Id, "oauth server");
                return Json(new { Token = authResult.SessionToken });
            }

            return Json(new { Token = token});
        }

        [HttpGet("~/getprivatekey")]
        public async Task<IActionResult> GetPrivateKey()
        {
            var applicationId = HttpContext.Request.Headers["application_id"].ToString();
            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null) return Json("Application Id Incorrect!");

            var clientId = User.GetClaim(ClaimTypes.NameIdentifier);
            string encodedPrivateKey = string.Empty;

            if (clientId != null)
            {
                var walletCredential = await _walletCredentialsRepository.GetAsync(clientId);

                return Json(new { EncodedPrivateKey = walletCredential?.EncodedPrivateKey });
            }

            return Json(new { EncodedPrivateKey = encodedPrivateKey });
        }
    }
}
