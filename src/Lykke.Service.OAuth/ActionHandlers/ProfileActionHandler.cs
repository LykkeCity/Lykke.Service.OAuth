using System.Threading.Tasks;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;

namespace WebAuth.ActionHandlers
{
    public class ProfileActionHandler
    {
        private readonly IPersonalDataService _personalDataService;

        public ProfileActionHandler(IPersonalDataService personalDataService)
        {
            _personalDataService = personalDataService;
        }

        public async Task UpdatePersonalInformation(string clientId, string firstName, string lastName, string phone)
        {
            var fullname = $"{firstName} {lastName}";

            await _personalDataService.UpdateAsync(new PersonalDataModel
            {
                Id = clientId,
                FirstName = firstName,
                LastName = lastName,
                FullName = fullname,
                ContactPhone = phone
            });
                }
        }
    }
