using System.Threading.Tasks;

namespace Core.Clients
{
    public interface ITemporaryIdRepository
    {
        Task<string> GenerateTemporaryId(string realId);
        Task<string> GetRealId(string temporaryId);
        Task RemoveTemporaryIdRecord(string temporaryId);
    }
}
