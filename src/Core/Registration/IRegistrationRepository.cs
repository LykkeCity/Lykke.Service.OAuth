using System.Threading.Tasks;

namespace Core.Registration
{
    public interface IRegistrationRepository
    {
        Task<string> AddAsync(RegistrationInternalEntity registrationModel);
        Task<RegistrationInternalEntity> GetAsync(string key);
    }
}
