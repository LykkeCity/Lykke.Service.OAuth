using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using Core.Application;
using Core.Bitcoin;
using Core.Clients;
using Core.Kyc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Models;
using WebAuth.Managers;
using WebAuth.Models.UserProfile;
using Core.UserProfile;
using System.Collections.Generic;

namespace WebAuth.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IKycRepository _kycRepository;
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly IClientsSessionsRepository _clientsSessionsRepository;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly IUserManager _userManager;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IPersonalDataService _personalDataService;

        public UserProfileController(
            IApplicationRepository applicationRepository,
            IKycRepository kycRepository,
            IClientAccountsRepository clientAccountsRepository,
            IClientsSessionsRepository clientsSessionsRepository,
            IWalletCredentialsRepository walletCredentialsRepository,
            IUserManager userManager,
            IUserProfileRepository userProfileRepository,
            IPersonalDataService personalDataService
            )
        {
            _applicationRepository = applicationRepository;
            _kycRepository = kycRepository;
            _clientAccountsRepository = clientAccountsRepository;
            _clientsSessionsRepository = clientsSessionsRepository;
            _walletCredentialsRepository = walletCredentialsRepository;
            _userManager = userManager;
            _userProfileRepository = userProfileRepository;
            _personalDataService = personalDataService;
        }

        [HttpGet("~/userprofile/{id}")]
        public async Task<IActionResult> UserProfile(string id)
        {
            var client = await _clientAccountsRepository.GetByIdAsync(id);
            var currentUserId = "";

            var identity = User.Identity;
            if (identity.IsAuthenticated)
            {
                currentUserId = _userManager.GetCurrentUserId();
            }

            if (currentUserId == client.Id)
            {
                var profile = await _userProfileRepository.GetAsync(id);
                var personalData = await _personalDataService.GetAsync(id);

                if (profile == null)
                {
                    var userProfileModel = new UserProfileViewModel
                    {
                        UserId = id,
                        FirstName = personalData.FirstName,
                        LastName = personalData.LastName
                    };

                    return View("~/Views/UserProfile/UserProfile.cshtml", userProfileModel);
                }
                else
                {
                    return View("~/Views/UserProfile/UserProfile.cshtml", profile);
                }

            }
            else
            {
                return View("ProfileAccessDenied");
            }
        }

        [HttpGet("~/userprofile/edituserprofile/{id}")]
        public async Task<IActionResult> EditUserProfile(string id)
        {
            var userProfile = await GetUserProfileViewModel(id);

            if (userProfile == null)
            {
                var personalData = await _personalDataService.GetAsync(id);

                userProfile = new UserProfileViewModel
                {
                    UserId = id,
                    FirstName = personalData.FirstName,
                    LastName = personalData.LastName
                };
            }

            var currentUserId = _userManager.GetCurrentUserId();

            if (currentUserId != null && currentUserId == id)
            {
                return View("EditUserProfile", userProfile);
            }
            else
            {
                return View("ProfileEditDenied");
            }
        }

        [HttpPost("~/userprofile/edituserprofile/saveuserprofile")]
        public async Task<IActionResult> SaveUserProfile(UserProfileViewModel userProfile, bool receiveLykkeNewsletter = false)
        {
            var profile = await _userProfileRepository.GetAsync(userProfile.UserId);
            var personalData = await _personalDataService.GetAsync(userProfile.UserId);
            userProfile.FirstName = personalData.FirstName;
            userProfile.LastName = personalData.LastName;

            if (profile == null)
            {
                await _userProfileRepository.SaveAsync(userProfile);
            }
            else
            {
                profile.ReceiveLykkeNewsletter = receiveLykkeNewsletter;
                await _userProfileRepository.UpdateAsync(userProfile);
            }

            return RedirectToAction("UserProfile", "UserProfile", new { id = userProfile.UserId });
        }

        [HttpGet("~/getuserprofilebyid")]
        public async Task<IActionResult> GetUserProfileById(string id)
        {
            var applicationId = HttpContext.Request.Headers["application_id"].ToString();
            var app = await _applicationRepository.GetByIdAsync(applicationId);

            if (app == null) return Json("Application Id Incorrect!");

            var profile = await _userProfileRepository.GetAsync(id);
            if (profile == null)
            {
                var client = await _personalDataService.GetAsync(id);
                profile = new UserProfileViewModel
                {
                    FirstName = client.FirstName,
                    LastName = client.LastName
                };
            }

            return Json(profile);
        }

        private async Task<UserProfileViewModel> GetUserProfileViewModel(string id)
        {
            var userProfile = await _userProfileRepository.GetAsync(id);

            if (userProfile == null) return null;

            var model = new UserProfileViewModel
            {
                UserId = id,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                Bio = userProfile.Bio,
                FacebookLink = userProfile.FacebookLink,
                GithubLink = userProfile.GithubLink,
                TwitterLink = userProfile.TwitterLink,
                ReceiveLykkeNewsletter = userProfile.ReceiveLykkeNewsletter,
                Website = userProfile.Website
            };

            return model;
        }
    }
}
