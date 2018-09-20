using System.Collections.Generic;

namespace Core.Application
{
    public interface IApplication
    {
        string ApplicationId { get; }
        string DisplayName { get; }
        string RedirectUri { get; }
        string Secret { get; }
        string Type { get; }
        IReadOnlyCollection<GrantType> GrantTypes { get; }
    }
}
