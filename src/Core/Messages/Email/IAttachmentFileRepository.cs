using System.IO;
using System.Threading.Tasks;

namespace Core.Messages.Email
{
    public interface IAttachmentFileRepository
    {
        Task<string> InsertAttachment(Stream stream);
        Task<Stream> GetAttachment(string fileId);
    }
}