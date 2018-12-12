using System.Threading.Tasks;

namespace Core.ExternalProvider
{
    public interface IIroncladUserRepository
    {
        //TODO:@gafanasiev Add summary
        Task<bool> AddAsync(IroncladUser ironcladUser);

        //TODO:@gafanasiev Add summary
        Task<IroncladUser> GetByIdAsync(string ironcladUserId);
    }
}
