using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application
{
    public class Application : IApplication
    {
        public string ApplicationId { get; private set; }
        public string DisplayName { get; private set; }
        public string RedirectUri { get; private set; }
        public string Secret { get; private set; }
        public string Type { get; private set; }
        public IReadOnlyCollection<GrantType> GrantTypes { get; private set; }

        public IEnumerable<string> Urls => string.IsNullOrEmpty(RedirectUri?.Trim())
            ? Array.Empty<string>()
            : RedirectUri.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToArray();

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
                    Type = src.Type,
                    GrantTypes = src.GrantTypes
                };
        }
    }
}
