using System.Threading.Tasks;

namespace Core.Registration
{
    public interface IRegistrationRepository
    {
        Task<string> AddAsync(RegistrationModel registrationModel);
        Task<string> UpdateAsync(RegistrationModel registrationModel);

        Task<RegistrationModel> GetAsync(string registrationId);
        Task<RegistrationModel> GetAsync(string email, string password);
        Task<bool> IsEmailTaken(string email);
    }
}
