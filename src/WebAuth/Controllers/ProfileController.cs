using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAuth.ActionHandlers;
using WebAuth.Models.Profile;

namespace WebAuth.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly ProfileActionHandler _profileActionHandler;

        public ProfileController(ProfileActionHandler profileActionHandler)
        {
            _profileActionHandler = profileActionHandler;
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

        [HttpGet("~/country-of-residence")]
        public async Task<ActionResult> CountryOfResidence(string returnUrl = null)
        {
            var model = await _profileActionHandler.GetCountryOfResidence(returnUrl);

            return View("CountryOfResidence", model);
        }

        [HttpPost("~/country-of-residence")]
        public async Task<ActionResult> CountryOfResidence(CountryOfResidenceViewModel model)
        {
            await _profileActionHandler.UpdateCountryOfResidence(model);

            return RedirectToAction("AddressInformation", new {returnUrl = model.ReturnUrl});
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