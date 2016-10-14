using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Clients;

namespace Core.Kyc
{
    public interface ISrvKycManager
    {
        Task<string> UploadDocument(string clientId, string type, string fileName, string mime, byte[] data,
            string changer);

        Task<IKycDocument> DeleteAsync(string clientId, string documentId, string changer);
        Task<bool> ChangeKycStatus(string clientId, KycStatus kycStatus, string changer);
        Task<IEnumerable<IPersonalData>> GetAccountsToCheck();

        Task<IClientAccount> RegisterClientAsync(string email, string firstName, string lastName, string phone, string password, string hint, string clientInfo, string ip, string changer, string language);

        Task UpdatePersonalDataAsync(IPersonalData personalData, string changer);
        Task ChangePhoneAsync(string clientId, string phoneNumber, string changer);
        Task ChangeFullNameAsync(string clientId, string fullName, string changer);
        Task ChangeFirstNameAsync(string clientId, string firstName, string changer);
        Task ChangeLastNameAsync(string clientId, string lastName, string changer);
        Task ChangeZipAsync(string clientId, string zip, string changer);
        Task ChangeCityAsync(string clientId, string city, string changer);
        Task ChangeAddressAsync(string clientId, string address, string changer);
        Task ChangeCountryAsync(string clientId, string country, string changer);
    }
}