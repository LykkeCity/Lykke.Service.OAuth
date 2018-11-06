using System.Threading.Tasks;

namespace Core.Registration
{
    public interface IRegistrationRepository
    {
        Task<string> AddAsync(RegistrationModel registrationModel);
        Task<string> UpdateAsync(RegistrationModel registrationModel);

        Task<RegistrationModel> GetByIdAsync(string registrationId);
        Task<RegistrationModel> GetByEmailAsync(string email);
    }
}
