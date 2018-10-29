using System.Threading.Tasks;

namespace Core.Registration
{
    public interface IRegistrationRepository
    {
        Task<string> AddAsync(RegistrationModel registrationModel);
        Task<RegistrationModel> GetAsync(string registrationId);
    }
}
