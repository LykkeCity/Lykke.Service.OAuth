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

        public ApplicationCachedRepository(IApplicationRepository repository, IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public Task<Core.Application.Application> GetByIdAsync(string id)
        {
            return _cache.GetOrCreateAsync(id, entry =>
              {
                  entry.SlidingExpiration = TimeSpan.FromMinutes(1);
                  return _repository.GetByIdAsync(id);
              });
        }
    }
}
