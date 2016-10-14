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
    }

    public class Application : IApplication
    {
        public string ApplicationId { get; set; }
        public string DisplayName { get; set; }
        public string RedirectUri { get; set; }
        public string Secret { get; set; }

        public static Application Create(string displayName, string redirectUrl, string secret)
        {
            return new Application
            {
                DisplayName = displayName,
                RedirectUri = redirectUrl,
                Secret = secret
            };
        }

        public static Application CreateDefault()
        {
            return new Application();
        }
    }

    public interface IApplicationRepository
    {
        Task<IApplication> GetByIdAsync(string id);
        Task<IEnumerable<IApplication>> GetApplicationsAsync();
        Task RegisterApplicationAsync(IApplication application);
        Task EditApplicationAsync(string id, IApplication application);
        Task DeleteAsync(string id);
    }
}