using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Partner
{
    public interface IPartnerAccountPolicy
    {
        string PublicId { get; set; }
        bool UseDifferentCredentials { get; set; }
        bool UseDifferentWallets { get; set; }
    }

    public class PartnerAccountPolicy : IPartnerAccountPolicy
    {
        public string PublicId { get; set; }

        public bool UseDifferentCredentials { get; set; }

        public bool UseDifferentWallets { get; set; }
    }

    public interface IPartnerAccountPolicyRepository
    {
        Task CreateAsync(IPartnerAccountPolicy partner);
        Task CreateOrUpdateAsync(IPartnerAccountPolicy partner);
        Task UpdateAsync(IPartnerAccountPolicy partner);
        Task<IEnumerable<IPartnerAccountPolicy>> GetPoliciesAsync();
        Task<IPartnerAccountPolicy> GetAsync(string publicId);
        Task RemoveAsync(string publicId);
    }
}
