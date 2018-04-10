using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Application
{
    public interface IApplication
    {
        string ApplicationId { get; }
        string DisplayName { get; }
        string RedirectUri { get; }
        string Secret { get; }
        string Type { get; set; }
    }

    public class Application : IApplication
    {
        public string ApplicationId { get; set; }
        public string DisplayName { get; set; }
        public string RedirectUri { get; set; }
        public string Secret { get; set; }
        public string Type { get; set; }        
    }

    public interface IApplicationRepository
    {
        Task<IApplication> GetByIdAsync(string id);        
    }
}
