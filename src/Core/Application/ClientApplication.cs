using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application
{
    public class ClientApplication : IApplication
    {
        public string ApplicationId { get; private set; }
        public string DisplayName { get; private set; }
        public string RedirectUri { get; private set; }
        public string Secret { get; private set; }
        public string Type { get; private set; }
        public OAuthClientProperties OAuthClientProperties { get; private set; }

        public IEnumerable<string> Urls => string.IsNullOrEmpty(RedirectUri?.Trim())
            ? Array.Empty<string>()
            : RedirectUri.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToArray();

        public static ClientApplication Create(IApplication src)
        {
            return src == null
                ? null
                : new ClientApplication
                {
                    ApplicationId = src.ApplicationId,
                    DisplayName = src.DisplayName,
                    RedirectUri = src.RedirectUri,
                    Secret = src.Secret,
                    Type = src.Type,
                    OAuthClientProperties = src.OAuthClientProperties
                };
        }
    }
}
