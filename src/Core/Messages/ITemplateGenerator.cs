using System.Threading.Tasks;

namespace Core.Messages
{
    public enum TemplateType
    {
        Sms,
        Email
    }

    public interface ITemplateGenerator
    {
        Task<string> GenerateAsync<T>(string templateName, T templateVm, TemplateType type);
    }
}
