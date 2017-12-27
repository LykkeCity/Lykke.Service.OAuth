using System.Threading.Tasks;

namespace Core.Kyc
{
    public interface ISrvKycManager
    {
        Task<string> UploadDocument(string clientId, string type, string fileName, string mime, byte[] data,
            string changer);

        Task<bool> ChangeKycStatus(string clientId, KycStatus kycStatus, string changer);

        Task ChangePhoneAsync(string clientId, string phoneNumber, string changer);
        Task ChangeFirstNameAsync(string clientId, string firstName, string changer);
        Task ChangeLastNameAsync(string clientId, string lastName, string changer);
        Task ChangeFullNameAsync(string clientId, string fullName, string changer);
        Task ChangeZipAsync(string clientId, string zip, string changer);
        Task ChangeCityAsync(string clientId, string city, string changer);
        Task ChangeAddressAsync(string clientId, string address, string changer);
        Task ChangeCountryAsync(string clientId, string country, string changer);
    }
}