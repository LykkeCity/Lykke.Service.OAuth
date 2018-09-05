using System.Security.Claims;
using System.Threading.Tasks;
using Core.Extensions;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using WebAuth.Managers;

namespace WebAuth.ActionHandlers
{
    public class ProfileActionHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserManager _userManager;
        private readonly IPersonalDataService _personalDataService;

        public ProfileActionHandler(
            IHttpContextAccessor httpContextAccessor,
            IUserManager userManager,
            IPersonalDataService personalDataService
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _personalDataService = personalDataService;
        }

        public async Task UpdatePersonalInformation(string clientId, string firstName, string lastName)
        {
            var fullname = $"{firstName} {lastName}";

            await _personalDataService.UpdateAsync(new PersonalDataModel
            {
                Id = clientId,
                FirstName = firstName,
                LastName = lastName,
                FullName = fullname
            });
        }
    }
}
