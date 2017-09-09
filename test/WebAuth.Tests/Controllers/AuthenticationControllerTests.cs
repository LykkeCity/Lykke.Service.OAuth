using System.Threading.Tasks;
using Common.Log;
using Core.Clients;
using Lykke.Service.Registration;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WebAuth.Controllers;
using WebAuth.Managers;
using WebAuth.Models;
using Xunit;

namespace WebAuth.Tests.Controllers
{
    public class AuthenticationControllerTests
    {
        [Fact]
        public async Task RegisterPost_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            //arrange
            var registrationViewModel = new RegistrationViewModel();

            //act
            var controller = CreateAuthenticationController();
            controller.ModelState.AddModelError("Email", "Is required");

            var result = await controller.Register(registrationViewModel);

            //assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task SigninPost_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            //arrange
            var signinViewModel = new SigninViewModel();

            //act
            var controller = CreateAuthenticationController();
            controller.ModelState.AddModelError("Email", "Is required");

            var result = await controller.Signin(signinViewModel);

            //assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task SigninPost_ReturnsErrorPage_WhenModelClientDoesntExist()
        {
            //arrange
            var signinViewModel = new SigninViewModel();

            //act
            var clientRepository = Substitute.For<IClientAccountsRepository>();
            clientRepository.AuthenticateAsync(null, null).Returns((IClientAccount)null);

            var controller = CreateAuthenticationController(clientRepository);
            controller.ModelState.AddModelError("Username", "E-mail is required");
            controller.ModelState.AddModelError("Password", "Password is required");

            var result = await controller.Signin(signinViewModel);
            
            //assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<LoginViewModel>(viewResult.ViewData.Model);
            Assert.False(viewResult.ViewData.ModelState.IsValid);
            Assert.Equal("Login", viewResult.ViewName);

        }

        private static AuthenticationController CreateAuthenticationController(IClientAccountsRepository clientRepository = null)
        {
            if (clientRepository == null)
                clientRepository = Substitute.For<IClientAccountsRepository>();

            var userManager = Substitute.For<IUserManager>();
            var log = Substitute.For<ILog>();
            var registrationClient = Substitute.For<ILykkeRegistrationClient>();

            return new AuthenticationController(registrationClient, clientRepository, userManager, log);
        }
    }
}
