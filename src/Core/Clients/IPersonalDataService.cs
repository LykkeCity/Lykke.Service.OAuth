using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IPersonalDataService
    {

        #region Get

        Task<IPersonalData> GetAsync(string id);
        Task<IFullPersonalData> GetFullAsync(string id);
        Task<IEnumerable<IPersonalData>> GetAsync(IEnumerable<string> id);
        Task<IEnumerable<IFullPersonalData>> GetFullAsync(IEnumerable<string> id);

        #endregion


        #region Search

        /// <summary>
        /// Find clients by email
        /// </summary>
        Task<IPersonalData> FindClientsByEmail(string email);

        /// <summary>
        /// Search client by part of full name, email or contact phone
        /// </summary>
        Task<IPersonalData> FindClientsByPhrase(string phrase);

        #endregion


        #region Modify

        Task SaveAsync(IFullPersonalData personalData);
        Task UpdateAsync(IPersonalData personalData);
        Task ChangeFullNameAsync(string id, string fullName);
        Task ChangeFirstNameAsync(string id, string firstName);
        Task ChangeLastNameAsync(string id, string lastName);
        Task ChangeCountryAsync(string id, string country);
        Task ChangeCityAsync(string id, string city);
        Task ChangeZipAsync(string id, string zip);
        Task ChangeAddressAsync(string id, string zip);
        Task ChangeContactPhoneAsync(string id, string phoneNumber);
        Task UpdateGeolocationDataAsync(string id, string countryCode, string city);
        Task ChangePasswordHintAsync(string id, string newHint);
        Task SetReferralCodeAsync(string id, string refCode);
        Task ChangeSpotRegulatorAsync(string id, string spotRegulator);
        Task ChangeMarginRegulatorAsync(string id, string marginRegulator);

        #endregion

    }
}
