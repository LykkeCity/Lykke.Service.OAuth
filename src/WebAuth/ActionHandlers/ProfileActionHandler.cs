using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessService.Kyc;
using Core.Clients;
using Core.Country;
using Core.Kyc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using WebAuth.Managers;
using WebAuth.Models;
using WebAuth.Models.Profile;

namespace WebAuth.ActionHandlers
{
    public class ProfileActionHandler
    {
        private const int TotalNumberOfFields = 12;
        private readonly AuthenticationActionHandler _authenticationActionHandler;
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly ICountryService _countryService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IKycDocumentsRepository _kycDocumentsRepository;
        private readonly IPersonalDataRepository _personalDataRepository;
        private readonly ISrvKycManager _srvKycManager;
        private readonly IUrlHelper _urlHelper;
        private readonly IUserManager _userManager;

        public ProfileActionHandler(ISrvKycManager srvKycManager, IPersonalDataRepository personalDataRepository,
            IKycDocumentsRepository kycDocumentsRepository, AuthenticationActionHandler authenticationActionHandler,
            IHttpContextAccessor httpContextAccessor, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor, IUserManager userManager,
            IClientAccountsRepository clientAccountsRepository, ICountryService countryService)
        {
            _srvKycManager = srvKycManager;
            _personalDataRepository = personalDataRepository;
            _kycDocumentsRepository = kycDocumentsRepository;
            _authenticationActionHandler = authenticationActionHandler;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _clientAccountsRepository = clientAccountsRepository;
            _countryService = countryService;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        private string CurrentClientId => _userManager.GetCurrentUserId();

        public async Task<PersonalInformationViewModel> GetPersonalInformation(string returnUrl)
        {
            var userFullData = await _personalDataRepository.GetAsync(CurrentClientId);

            var model = Mapper.Map<PersonalInformationViewModel>(userFullData) ?? new PersonalInformationViewModel();
            model.ReturnUrl = returnUrl;
            model.NextStepUrl = GetStepUrl("CountryOfResidence", returnUrl);
            model.PrevStepUrl = null;

            return model;
        }

        public async Task UpdatePersonalInformation(PersonalInformationViewModel viewModel)
        {
            await _srvKycManager.ChangeFirstNameAsync(CurrentClientId, viewModel.FirstName, RecordChanger.Client);
            await _srvKycManager.ChangeLastNameAsync(CurrentClientId, viewModel.LastName, RecordChanger.Client);
            await _srvKycManager.ChangePhoneAsync(CurrentClientId, viewModel.ContactPhone, RecordChanger.Client);

            //update client identity
            var clientAccount =
                await _clientAccountsRepository.GetByIdAsync(CurrentClientId);

            await
                _httpContextAccessor.HttpContext.Authentication.SignOutAsync("ServerCookie",
                    new AuthenticationProperties());

            var identity = await _userManager.CreateUserIdentityAsync(clientAccount, clientAccount.Email);
            await
                _httpContextAccessor.HttpContext.Authentication.SignInAsync("ServerCookie",
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties());
        }

        public async Task<CountryOfResidenceViewModel> GetCountryOfResidence(string returnUrl = null)
        {
            var userFullData = await _personalDataRepository.GetAsync(CurrentClientId);

            var model = Mapper.Map<CountryOfResidenceViewModel>(userFullData) ?? new CountryOfResidenceViewModel();
            model.ReturnUrl = returnUrl;
            model.NextStepUrl = GetStepUrl("AddressInformation", returnUrl);
            model.PrevStepUrl = GetStepUrl("PersonalInformation", returnUrl);

            var currentCulture = CultureInfo.CurrentCulture;
            var availableCountryList = await _countryService.GetCountryListAsync(currentCulture.Name);

            if (availableCountryList != null)
            {
                model.Countries =
                    availableCountryList.Select(
                            x =>
                                new SelectListItem
                                {
                                    Text = x.Name,
                                    Value = x.Iso3,
                                    Selected = x.Iso3.Equals(model.Country)
                                })
                        .ToList();
            }

            return model;
        }

        public async Task UpdateCountryOfResidence(CountryOfResidenceViewModel model)
        {
            await _srvKycManager.ChangeCountryAsync(CurrentClientId, model.Country, RecordChanger.Client);
        }

        public async Task<AddressInformationViewModel> GetAddressInformation(string returnUrl = null)
        {
            var userFullData = await _personalDataRepository.GetAsync(CurrentClientId);

            var model = Mapper.Map<AddressInformationViewModel>(userFullData) ?? new AddressInformationViewModel();
            model.ReturnUrl = returnUrl;
            model.NextStepUrl = GetStepUrl("ProofOfAddress", returnUrl);
            model.PrevStepUrl = GetStepUrl("CountryOfResidence", returnUrl);

            var idCard = await _kycDocumentsRepository.GetAsync(CurrentClientId);
            if (idCard != null)
            {
                var idCardDocumentName = idCard.GetFileNameByType(KycDocumentTypes.IdCard);
                model.IdCard.DocumentName = idCardDocumentName;
                model.IdCard.DocumentMime = GetDocumentExtensionByName(idCardDocumentName);
            }

            return model;
        }

        public async Task UpdateAddressInformation(AddressInformationViewModel model)
        {
            model.IdCard.DocumentType = KycDocumentTypes.IdCard;

            await _srvKycManager.ChangeCityAsync(CurrentClientId, model.City, RecordChanger.Client);
            await _srvKycManager.ChangeAddressAsync(CurrentClientId, model.Address, RecordChanger.Client);
            await _srvKycManager.ChangeZipAsync(CurrentClientId, model.Zip, RecordChanger.Client);

            await _authenticationActionHandler.UploadFileAsync(model.IdCard, CurrentClientId);
        }

        public async Task<ProofOfAddressViewModel> GetProofOfAddress(string returnUrl = null)
        {
            var model = new ProofOfAddressViewModel {ReturnUrl = returnUrl};

            model.ReturnUrl = returnUrl;
            model.NextStepUrl = GetStepUrl("BankAccount", returnUrl);
            model.PrevStepUrl = GetStepUrl("AddressInformation", returnUrl);

            var proofOfAddress = await _kycDocumentsRepository.GetAsync(CurrentClientId);
            if (proofOfAddress != null)
            {
                var proofOfAddressFileName = proofOfAddress.GetFileNameByType(KycDocumentTypes.ProofOfAddress);
                model.ProofOfAddress.DocumentName = proofOfAddressFileName;
                model.ProofOfAddress.DocumentMime = GetDocumentExtensionByName(proofOfAddressFileName);
            }

            return model;
        }

        public async Task UpdateProofOfAddress(ProofOfAddressViewModel model)
        {
            model.ProofOfAddress.DocumentType = KycDocumentTypes.ProofOfAddress;

            await _authenticationActionHandler.UploadFileAsync(model.ProofOfAddress, CurrentClientId);
        }

        public async Task<BankAccountInfoViewModel> GetBankAccount(string returnUrl = null)
        {
            var model = new BankAccountInfoViewModel
            {
                ReturnUrl = returnUrl,
                NextStepUrl = GetStepUrl("AdditionalDocuments", returnUrl),
                PrevStepUrl = GetStepUrl("ProofOfAddress", returnUrl)
            };

            var documents = await _kycDocumentsRepository.GetAsync(CurrentClientId);
            if (documents != null)
            {
                var fundsDocumentName = documents.GetFileNameByType(KycDocumentTypes.ProofOfFunds);
                model.Funds.DocumentName = fundsDocumentName;
                model.Funds.DocumentMime = GetDocumentExtensionByName(fundsDocumentName);

                var bankAccountDocumentName = documents.GetFileNameByType(KycDocumentTypes.BankAccount);
                model.BankAccount.DocumentName = bankAccountDocumentName;
                model.BankAccount.DocumentMime = GetDocumentExtensionByName(bankAccountDocumentName);
            }

            return model;
        }

        public async Task UpdateBankAccount(BankAccountInfoViewModel model)
        {
            model.Funds.DocumentType = KycDocumentTypes.ProofOfFunds;
            model.BankAccount.DocumentType = KycDocumentTypes.BankAccount;

            await _authenticationActionHandler.UploadFileAsync(model.Funds, CurrentClientId);
            await _authenticationActionHandler.UploadFileAsync(model.BankAccount, CurrentClientId);
        }

        public async Task<AdditionalDocumentsViewModel> GetAdditionalDocuments(string returnUrl = null)
        {
            var model = new AdditionalDocumentsViewModel
            {
                ReturnUrl = returnUrl,
                NextStepUrl = null,
                PrevStepUrl = GetStepUrl("BankAccount", returnUrl)
            };

            var documents = await _kycDocumentsRepository.GetAsync(CurrentClientId);
            if (documents != null)
            {
                var firstDocumentDocumentName = documents.GetFileNameByType(KycDocumentTypes.AdditionalDocuments);
                model.FirstDocument.DocumentName = firstDocumentDocumentName;
                model.FirstDocument.DocumentMime = GetDocumentExtensionByName(firstDocumentDocumentName);
            }

            return model;
        }

        public async Task UpdateAdditionalDocuments(AdditionalDocumentsViewModel model)
        {
            model.FirstDocument.DocumentType = KycDocumentTypes.AdditionalDocuments;

            await _authenticationActionHandler.UploadFileAsync(model.FirstDocument, CurrentClientId);
//            await _authenticationActionHandler.UploadFileAsync(model.SecondDocument, clientId);
        }

        public async Task<StatusBarViewModel> GetStatusBarModelAsync()
        {
            var contactFullInfo = await _personalDataRepository.GetAsync(CurrentClientId);
            var documents = await _kycDocumentsRepository.GetAsync(CurrentClientId);

            var completionPercentage = CalculateCompletionPercentage(contactFullInfo, documents);

            return new StatusBarViewModel {CompletionPercentage = $"{completionPercentage}%"};
        }

        private static int CalculateCompletionPercentage(IPersonalData contactFullInfo,
            IEnumerable<IKycDocument> documents)
        {
            var filledData = 0;

            if (contactFullInfo != null)
            {
                filledData += Convert.ToInt32(!string.IsNullOrEmpty(contactFullInfo.FirstName)) +
                              Convert.ToInt32(!string.IsNullOrEmpty(contactFullInfo.LastName)) +
                              Convert.ToInt32(!string.IsNullOrEmpty(contactFullInfo.ContactPhone)) +
                              Convert.ToInt32(!string.IsNullOrEmpty(contactFullInfo.Country)) +
                              Convert.ToInt32(!string.IsNullOrEmpty(contactFullInfo.City)) +
                              Convert.ToInt32(!string.IsNullOrEmpty(contactFullInfo.Address)) +
                              Convert.ToInt32(!string.IsNullOrEmpty(contactFullInfo.Zip));
            }

            if (documents != null)
            {
                var kycDocuments = documents as IList<IKycDocument> ?? documents.ToList();
                filledData += Convert.ToInt32(kycDocuments.HasType(KycDocumentTypes.IdCard)) +
                              Convert.ToInt32(kycDocuments.HasType(KycDocumentTypes.ProofOfAddress)) +
                              Convert.ToInt32(kycDocuments.HasType(KycDocumentTypes.ProofOfFunds)) +
                              Convert.ToInt32(kycDocuments.HasType(KycDocumentTypes.BankAccount)) +
                              Convert.ToInt32(kycDocuments.HasType(KycDocumentTypes.AdditionalDocuments));
            }

            var completionPercentage = (int) Math.Round((double) filledData/TotalNumberOfFields*100);
            return completionPercentage;
        }

        private string GetStepUrl(string actionName, string returnUrl)
        {
            return _urlHelper.Action(actionName, new {returnUrl});
        }

        private static string GetDocumentExtensionByName(string documentName)
        {
            if (string.IsNullOrEmpty(documentName))
                return null;

            var documentExtension = documentName.Split('.').LastOrDefault();

            if (!KycDocumentTypes.GetDocumentExtenstionList().Contains(documentExtension))
            {
                documentExtension = KycDocumentTypes.GetDocumentExtenstionList().FirstOrDefault();
            }

            return documentExtension;
        }
    }
}