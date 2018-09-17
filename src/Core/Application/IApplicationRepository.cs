using System.Threading.Tasks;

namespace Core.Application
{
    public interface IApplicationRepository
    {
        Task<Application> GetByIdAsync(string id);        
    }
}
