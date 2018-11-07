using System.IO;
using System.Threading.Tasks;
using Lykke.Service.OAuth.Models.Registration;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Managers;
using WebAuth.Models;

namespace WebAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    [Route("[controller]")]
    public class ProfileController : BaseController
    {
        private readonly IUserManager _userManager;
        private readonly IPersonalDataService _personalDataService;

        public ProfileController(IUserManager userManager, 
            IPersonalDataService personalDataService
            )
        {
            _userManager = userManager;
            _personalDataService = personalDataService;
        }

        [HttpPost]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("getpersonaldata")]
        [ValidateAntiForgeryToken]
        public async Task<ProfilePersonalDataModel> GetPersonalData()
        {
            var clientId = _userManager.GetCurrentUserId();
            var personalData = await _personalDataService.GetProfilePersonalDataAsync(clientId);
            return personalData.ToModel();
        }

        [HttpPost]
        [Route("savepersonaldata")]
        [ValidateAntiForgeryToken]
        public async Task SavePersonalData([FromBody]UpdateProfileInfoRequest model)
        {
            model.ClientId = _userManager.GetCurrentUserId();
            await _personalDataService.UpdateProfileAsync(model);
        }

        [HttpPost]
        [Route("uploadavatar")]
        [ValidateAntiForgeryToken]
        public async Task<string> UploadAvatar(IFormFile file, bool isPreview)
        {
            if (file != null && file.Length <= 3 * 1024 * 1024 && file.ContentType.Contains("image"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    byte[] image = memoryStream.ToArray();

                    return await _personalDataService.AddAvatarAsync(_userManager.GetCurrentUserId(), isPreview, image);
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
    }
}
