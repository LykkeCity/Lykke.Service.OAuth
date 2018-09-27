using System;
using System.Threading.Tasks;
using Core.Application;
using Microsoft.Extensions.Caching.Memory;

namespace AzureDataAccess.Application
{
    public class ApplicationCachedRepository : IApplicationRepository
    {
        private readonly IApplicationRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cachedEntityTtl = TimeSpan.FromMinutes(1);
        public ApplicationCachedRepository(IApplicationRepository repository, IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public Task<ClientApplication> GetByIdAsync(string id)
        {
            return _cache.GetOrCreateAsync(id, entry =>
              {
                  entry.AbsoluteExpirationRelativeToNow = _cachedEntityTtl;
                  return _repository.GetByIdAsync(id);
              });
        }
    }
}
