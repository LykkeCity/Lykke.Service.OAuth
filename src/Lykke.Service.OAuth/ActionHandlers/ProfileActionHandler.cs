using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Abstractions.Services.Models;
using Lykke.Service.PersonalData.Contract.Models;
using Newtonsoft.Json.Linq;

namespace WebAuth.ActionHandlers
{
    public class ProfileActionHandler
    {
        private readonly IKycProfileService _kycProfileService;

        public ProfileActionHandler(IKycProfileService kycProfileService)
        {
            _kycProfileService = kycProfileService;
        }

        public async Task UpdatePersonalInformation(string clientId, string firstName, string lastName)
        {
            string fullname = $"{firstName} {lastName}";

            var changes = new KycPersonalDataChanges
            {
                Changer = RecordChanger.Client,
                Items = new Dictionary<string, JToken>
                {
                    {nameof(IPersonalData.FirstName), firstName},
                    {nameof(IPersonalData.LastName), lastName},
                    {nameof(IPersonalData.FullName), fullname}
                }
            };

            await _kycProfileService.UpdatePersonalDataAsync(clientId, changes);


        }
    }
}
