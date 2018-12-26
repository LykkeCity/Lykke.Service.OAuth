using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Common.Log;
using Core.Extensions;
using Lykke.Logs;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.OAuth.Managers;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using WebAuth.Managers;
using Xunit;

namespace WebAuth.Tests.Managers
{
    public class UserManagerTests
    {
        private static UserManager CreateUserManager(IPersonalDataService personalDataService = null, IClientAccountClient clientAccountClient = null)
        {
            if (personalDataService == null)
            {
                personalDataService = Substitute.For<IPersonalDataService>();
            }

            var httpAccessor = Substitute.For<IHttpContextAccessor>();
            var userManager = new UserManager(personalDataService, httpAccessor, clientAccountClient);
            return userManager;
        }

        [Fact]
        public async Task Email_IsRequired_ForUserIdentity()
        {
            var personalData = new FullPersonalDataModel();

            var clientAccountModel = new ClientModel();

            //act
            var personalDataService = Substitute.For<IPersonalDataService>();
            personalDataService.GetAsync(Arg.Any<string>()).ReturnsForAnyArgs(personalData);

            var clientAccountClient = Substitute.For<IClientAccountClient>();
            clientAccountClient.GetByIdAsync(Arg.Any<string>()).ReturnsForAnyArgs(clientAccountModel);

            var userManager = CreateUserManager(personalDataService, clientAccountClient);

            //assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => userManager.CreateUserIdentityAsync("test", null, null, null, null));

        }

        [Fact]
        public void Identity_ShouldContainCountry_IfScopeIsAddress()
        {
            //arrange
            var scopes = new List<string>
            {
                OpenIdConnectConstants.Scopes.Address
            };

            var country = "Test";
            var claims = new List<Claim>
            {
                new Claim(OpenIdConnectConstantsExt.Claims.Country, country)
            };

            //act
            var userManager = CreateUserManager();
            var result = userManager.CreateIdentity(scopes, claims);

            //assert
            Assert.Equal(country, result.GetClaim(OpenIdConnectConstantsExt.Claims.Country));
        }

        [Fact]
        public void Identity_ShouldContainData_ForSpecifiedScope()
        {
            //arrange
            var scopes = new List<string>
            {
                OpenIdConnectConstants.Scopes.Email,
                OpenIdConnectConstants.Scopes.Profile
            };
            var claims = new List<Claim>
            {
                new Claim(OpenIdConnectConstants.Claims.Email, "test@test.com"),
                new Claim(OpenIdConnectConstants.Claims.FamilyName, "Smith")
            };

            //act
            var userManager = CreateUserManager();
            var result = userManager.CreateIdentity(scopes, claims);

            //assert
            Assert.Equal("test@test.com", result.GetClaim(OpenIdConnectConstants.Claims.Email));
            Assert.Equal("Smith", result.GetClaim(OpenIdConnectConstants.Claims.FamilyName));
        }



        [Fact]
        public void Identity_ShouldNotContainEmail_IfScopeContainsProfileOnly()
        {
            //arrange
            var scopes = new List<string>
            {
                OpenIdConnectConstants.Scopes.Profile
            };
            var claims = new List<Claim>
            {
                new Claim(OpenIdConnectConstants.Claims.Email, "test@test.com")
            };

            //act
            var userManager = CreateUserManager();
            var result = userManager.CreateIdentity(scopes, claims);

            //assert
            Assert.Null(result.GetClaim(OpenIdConnectConstants.Claims.Email));
        }

        [Fact]
        public void Identity_ShouldNotContainEmail_IfScopeIsEmpty()
        {
            //arrange
            var scopes = new List<string>();
            var claims = new List<Claim>
            {
                new Claim(OpenIdConnectConstants.Claims.Email, "test@test.com")
            };

            //act
            var userManager = CreateUserManager();
            var result = userManager.CreateIdentity(scopes, claims);

            //assert
            Assert.Null(result.GetClaim(OpenIdConnectConstants.Claims.Email));
        }

        [Fact]
        public void Identity_ShouldNotContainFirstOrLastName_IfScopeContainsEmailOnly()
        {
            //arrange
            var scopes = new List<string>
            {
                OpenIdConnectConstants.Scopes.Email
            };
            var claims = new List<Claim>
            {
                new Claim(OpenIdConnectConstants.Claims.GivenName, "John"),
                new Claim(OpenIdConnectConstants.Claims.FamilyName, "Smith")
            };

            //act
            var userManager = CreateUserManager();
            var result = userManager.CreateIdentity(scopes, claims);

            //assert
            Assert.Null(result.GetClaim(OpenIdConnectConstants.Claims.GivenName));
            Assert.Null(result.GetClaim(OpenIdConnectConstants.Claims.FamilyName));
        }

        [Fact]
        public void Identity_ShouldNotContainFirstOrLastName_IfScopeIsEmpty()
        {
            //arrange
            var scopes = new List<string>();
            var claims = new List<Claim>
            {
                new Claim(OpenIdConnectConstants.Claims.GivenName, "John"),
                new Claim(OpenIdConnectConstants.Claims.FamilyName, "Smith")
            };

            //act
            var userManager = CreateUserManager();
            var result = userManager.CreateIdentity(scopes, claims);

            //assert
            Assert.Null(result.GetClaim(OpenIdConnectConstants.Claims.GivenName));
            Assert.Null(result.GetClaim(OpenIdConnectConstants.Claims.FamilyName));
        }

        [Fact]
        public async Task OptionalFields_AreAdded_ToUserIdentity()
        {
            //arrange
            var personalData = new FullPersonalDataModel
            {
                FirstName = "test",
                LastName = "test",
                ContactPhone = "11",
                Country = "Test"
            };

            var clientAccountModel = new ClientModel();

            //act
            var personalDataService = Substitute.For<IPersonalDataService>();
            personalDataService.GetAsync(Arg.Any<string>()).ReturnsForAnyArgs(personalData);

            var clientAccountClient = Substitute.For<IClientAccountClient>();
            clientAccountClient.GetByIdAsync(Arg.Any<string>()).ReturnsForAnyArgs(clientAccountModel);

            var userManager = CreateUserManager(personalDataService, clientAccountClient);
            var result = await userManager.CreateUserIdentityAsync("test", "test@test.com", "fdfd", "test", "test");

            //assert
            Assert.Equal(personalData.FirstName, result.GetClaim(OpenIdConnectConstants.Claims.GivenName));
            Assert.Equal(personalData.LastName, result.GetClaim(OpenIdConnectConstants.Claims.FamilyName));
            Assert.Equal("test@test.com", result.GetClaim(OpenIdConnectConstants.Claims.Email));
            Assert.Equal(personalData.Country, result.GetClaim(OpenIdConnectConstantsExt.Claims.Country));
            Assert.Equal(12, result.Claims.Count());
        }
    }
}
