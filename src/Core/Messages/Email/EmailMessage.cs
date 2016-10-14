using System.IO;

namespace Core.Messages.Email
{
    public class EmailAttachment
    {
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public Stream Stream { get; set; }    
    }

    public class EmailMessage
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public EmailAttachment[] Attachments { get; set; }
    }
}
