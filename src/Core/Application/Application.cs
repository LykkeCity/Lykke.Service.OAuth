using System;
using System.Linq;

namespace Core.Application
{
    public class Application : IApplication
    {
        public string ApplicationId { get; set; }
        public string DisplayName { get; set; }
        public string RedirectUri { get; set; }
        public string Secret { get; set; }
        public string Type { get; set; }

        public string[] Urls => string.IsNullOrEmpty(RedirectUri?.Trim()) 
            ? Array.Empty<string>()
            : RedirectUri.Split(new []{',', ';'}, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToArray();

        public static Application Create(IApplication src)
        {
            return src == null 
                ? null 
                : new Application
                {
                    ApplicationId = src.ApplicationId,
                    DisplayName = src.DisplayName,
                    RedirectUri = src.RedirectUri,
                    Secret = src.Secret,
                    Type = src.Type
                };
        }
    }
}
