using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Clients;
using Core.Country;
using Core.Kyc;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using NSubstitute;
using WebAuth.ActionHandlers;
using WebAuth.Managers;
using Xunit;

namespace WebAuth.Tests.ActionHandlers
{
    public class ProfileActionHandlerTests
    {
        private ProfileActionHandler CreateProfileActionHandler(IPersonalDataService personalDataService,
            IKycDocumentsRepository kycDocumentsRepository)
        {
            var srvManager = Substitute.For<ISrvKycManager>();
            var personalDataRepo = personalDataService ?? Substitute.For<IPersonalDataService>();
            var kycDocRepo = kycDocumentsRepository ?? Substitute.For<IKycDocumentsRepository>();
            var kycRepo = Substitute.For<IKycRepository>();
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var urlHelperFactory = Substitute.For<IUrlHelperFactory>();
            var actionContextAccessor = Substitute.For<IActionContextAccessor>();
            var userManager = Substitute.For<IUserManager>();
            var countryService = Substitute.For<ICountryService>();
            var clientAccountRepo = Substitute.For<IClientAccountsRepository>();
            var clientSettingRepo = Substitute.For<IClientSettingsRepository>();
            var authActionHandler = new AuthenticationActionHandler(kycRepo, srvManager, clientSettingRepo, kycDocRepo);

            var fakeClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var fakeIdentity = Substitute.For<ClaimsIdentity>();
            fakeIdentity.FindFirst(ClaimTypes.NameIdentifier).Returns(fakeClaim);

            var fakeClaimsPrincipal = Substitute.For<ClaimsPrincipal>();
            fakeClaimsPrincipal.HasClaim(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            fakeClaimsPrincipal.Identity.Returns(fakeIdentity);
            fakeClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Returns(fakeClaim);

            var fakeHttpContext = Substitute.For<HttpContext>();
            fakeHttpContext.User.Returns(fakeClaimsPrincipal);
            httpContextAccessor.HttpContext.Returns(fakeHttpContext);
            httpContextAccessor.HttpContext.User.Returns(fakeClaimsPrincipal);

            return new ProfileActionHandler(srvManager, personalDataRepo, kycDocRepo, authActionHandler,
                httpContextAccessor, urlHelperFactory, actionContextAccessor, userManager, clientAccountRepo,
                countryService);
        }

        [Fact]
        public async Task CompletedAccount_CompletionPercentage_Is_Correct()
        {
            //data
            var fullPersonalData = new FullPersonalDataModel
            {
                FirstName = "test",
                LastName = "test",
                ContactPhone = "test",
                Country = "test",
                City = "test",
                Address = "test",
                Zip = "test"
            };

            var kycDocuments = new List<KycDocument>
            {
                new KycDocument {Type = KycDocumentTypes.IdCard},
                new KycDocument {Type = KycDocumentTypes.ProofOfAddress},
                new KycDocument {Type = KycDocumentTypes.ProofOfFunds},
                new KycDocument {Type = KycDocumentTypes.BankAccount},
                new KycDocument {Type = KycDocumentTypes.AdditionalDocuments}
            };

            var personalDataService = Substitute.For<IPersonalDataService>();
            personalDataService.GetAsync(Arg.Any<string>()).ReturnsForAnyArgs(fullPersonalData);
            var kycDocRepo = Substitute.For<IKycDocumentsRepository>();
            kycDocRepo.GetAsync(Arg.Any<string>()).ReturnsForAnyArgs(kycDocuments);

            var profileActionHandler = CreateProfileActionHandler(personalDataService, kycDocRepo);

            //act
            var completionPercentage = await profileActionHandler.GetStatusBarModelAsync();

            Assert.Equal("100%", completionPercentage.CompletionPercentage);
        }

        [Fact]
        public async Task EmptyAccount_CompletionPercentage_Is_Correct()
        {
            //data
            var fullPersonalData = new FullPersonalDataModel();

            var kycDocuments = new List<KycDocument>();

            var personalDataService = Substitute.For<IPersonalDataService>();
            personalDataService.GetAsync(Arg.Any<string>()).ReturnsForAnyArgs(fullPersonalData);
            var kycDocRepo = Substitute.For<IKycDocumentsRepository>();
            kycDocRepo.GetAsync(Arg.Any<string>()).ReturnsForAnyArgs(kycDocuments);

            var profileActionHandler = CreateProfileActionHandler(personalDataService, kycDocRepo);

            //act
            var completionPercentage = await profileActionHandler.GetStatusBarModelAsync();

            Assert.Equal("0%", completionPercentage.CompletionPercentage);
        }

        [Fact]
        public async Task HalfCompletedAccount_CompletionPercentage_Is_Correct()
        {
            //data
            var fullPersonalData = new FullPersonalDataModel
            {
                FirstName = "test",
                LastName = "test",
                City = "test",
                Address = "test"
            };

            var kycDocuments = new List<KycDocument>
            {
                new KycDocument {Type = KycDocumentTypes.ProofOfAddress},
                new KycDocument {Type = KycDocumentTypes.BankAccount}
            };

            var personalDataService = Substitute.For<IPersonalDataService>();
            personalDataService.GetAsync(Arg.Any<string>()).ReturnsForAnyArgs(fullPersonalData);
            var kycDocRepo = Substitute.For<IKycDocumentsRepository>();
            kycDocRepo.GetAsync(Arg.Any<string>()).ReturnsForAnyArgs(kycDocuments);

            var profileActionHandler = CreateProfileActionHandler(personalDataService, kycDocRepo);

            //act
            var completionPercentage = await profileActionHandler.GetStatusBarModelAsync();

            Assert.Equal("50%", completionPercentage.CompletionPercentage);
        }
    }
}