using System.Threading.Tasks;

namespace Core.Kyc
{
    public interface IKycDocumentsScansRepository
    {
        Task AddDocument(string id, byte[] data);
        Task<byte[]> GetDocument(string id);
    }
}