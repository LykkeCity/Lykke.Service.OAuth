using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Clients;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using WebAuth.ActionHandlers;
using WebAuth.Managers;
using WebAuth.Models.Profile;

namespace WebAuth.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ProfileController : BaseController
    {
        private readonly IUserManager _userManager;
        private readonly IPersonalDataService _personalDataService;
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly ProfileActionHandler _profileActionHandler;

        public ProfileController(IUserManager userManager, 
            IPersonalDataService personalDataService,
            IClientAccountsRepository clientAccountsRepository,
            ProfileActionHandler profileActionHandler)
        {
            _userManager = userManager;
            _personalDataService = personalDataService;
            _clientAccountsRepository = clientAccountsRepository;
            _profileActionHandler = profileActionHandler;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("getpersonaldata")]
        [ValidateAntiForgeryToken]
        public async Task<ProfilePersonalData> GetPersonalData()
        {
            var clientId = _userManager.GetCurrentUserId();
            return await _personalDataService.GetProfilePersonalDataAsync(clientId);
        }

        [HttpPost]
        [Route("savepersonaldata")]
        [ValidateAntiForgeryToken]
        public async Task SavePersonalData([FromBody]UpdateProfileInfoRequest model)
        {
            await _personalDataService.UpdateProfileAsync(model);

            var clientAccount = await _clientAccountsRepository.GetByIdAsync(model.ClientId);

            await HttpContext.Authentication.SignOutAsync("ServerCookie");
            var identity = await _userManager.CreateUserIdentityAsync(clientAccount.Id, clientAccount.Email, clientAccount.Email, true);
            await HttpContext.Authentication.SignInAsync("ServerCookie", new ClaimsPrincipal(identity), new AuthenticationProperties());
        }

        [HttpPost]
        [Route("uploadavatar")]
        [ValidateAntiForgeryToken]
        public async Task<string> UploadAvatar(IFormFile file)
        {
            if (file != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    byte[] image = memoryStream.ToArray();

                    return await _personalDataService.AddAvatarAsync(_userManager.GetCurrentUserId(), image);
                }
            }

            return null;
        }

        [HttpPost]
        [Route("deleteavatar")]
        [ValidateAntiForgeryToken]
        public async Task DeleteAvatar()
        {
            await _personalDataService.DeleteAvatarAsync(_userManager.GetCurrentUserId());
        }

        [HttpGet("~/personal-information")]
        public async Task<ActionResult> PersonalInformation(string returnUrl = null)
        {
            var model = await _profileActionHandler.GetPersonalInformation(returnUrl);

            return View("PersonalInformation", model);
        }

        [HttpPost("~/personal-information")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PersonalInformation(PersonalInformationViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                await _profileActionHandler.UpdatePersonalInformation(viewModel);

                return RedirectToLocal(viewModel.ReturnUrl);
            }

            return View("PersonalInformation", viewModel);
        }

        [HttpGet("~/address-information")]
        public async Task<ActionResult> AddressInformation(string returnUrl = null)
        {
            var model = await _profileActionHandler.GetAddressInformation(returnUrl);

            return View("AddressInformation", model);
        }

        [HttpPost("~/address-information")]
        public async Task<ActionResult> AddressInformation(AddressInformationViewModel model)
        {
            await _profileActionHandler.UpdateAddressInformation(model);

            return RedirectToAction("ProofOfAddress", new {returnUrl = model.ReturnUrl});
        }

        [HttpGet("~/proof-of-address")]
        public async Task<ActionResult> ProofOfAddress(string returnUrl = null)
        {
            var model = await _profileActionHandler.GetProofOfAddress(returnUrl);

            return View("ProofOfAddress", model);
        }

        [HttpPost("~/proof-of-address")]
        public async Task<ActionResult> ProofOfAddress(ProofOfAddressViewModel model)
        {
            await _profileActionHandler.UpdateProofOfAddress(model);

            return RedirectToAction("BankAccount", new {returnUrl = model.ReturnUrl});
        }

        [HttpGet("~/bank-account")]
        public async Task<ActionResult> BankAccount(string returnUrl = null)
        {
            var model = await _profileActionHandler.GetBankAccount(returnUrl);

            return View("BankAccount", model);
        }

        [HttpPost("~/bank-account")]
        public async Task<ActionResult> BankAccount(BankAccountInfoViewModel model)
        {
            await _profileActionHandler.UpdateBankAccount(model);

            return RedirectToAction("AdditionalDocuments", new {returnUrl = model.ReturnUrl});
        }

        [HttpGet("~/additional-documents")]
        public async Task<ActionResult> AdditionalDocuments(string returnUrl = null)
        {
            var model = await _profileActionHandler.GetAdditionalDocuments(returnUrl);

            return View("AdditionalDocuments", model);
        }

        [HttpPost("~/additional-documents")]
        public async Task<ActionResult> AdditionalDocuments(AdditionalDocumentsViewModel model)
        {
            await _profileActionHandler.UpdateAdditionalDocuments(model);

            return RedirectToLocal(model.ReturnUrl);
        }
    }
}