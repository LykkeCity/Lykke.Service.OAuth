using System.Threading.Tasks;

namespace Core.Application
{
    public interface IApplicationRepository
    {
        Task<ClientApplication> GetByIdAsync(string id);        
    }
}
