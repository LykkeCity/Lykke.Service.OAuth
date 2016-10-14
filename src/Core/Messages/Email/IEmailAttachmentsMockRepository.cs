using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Messages.Email
{
    public interface IEmailAttachmentsMock
    {
        string EmailMockId { get; }
        string AttachmentFileId { get; }  
        string FileName { get; }
        string ContentType { get; }
    }

    public class EmailAttachmentsMock : IEmailAttachmentsMock
    {
        public string EmailMockId { get; set;  }
        public string AttachmentFileId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }

    public interface IEmailAttachmentsMockRepository
    {
        Task InsertAsync(string emailMockId, string fileId, string fileName, string contentType);
        Task<IEnumerable<IEmailAttachmentsMock>> GetAsync(string emailMockId);
    }
}
