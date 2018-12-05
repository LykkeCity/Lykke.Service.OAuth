using System.Threading.Tasks;

namespace Core.ExternalProvider
{
    public interface IIroncladService
    {
        //TODO:@gafanasiev add summary
        Task AddClaim(string ironcladUserId, string type, string value);
    }
}
