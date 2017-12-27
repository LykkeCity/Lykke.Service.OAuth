using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Kyc
{
    public enum KycStatus
    {
        NeedToFillData,
        Pending,
        Ok,
        Rejected,
        RestrictedArea
    }

    public interface IKycRepository
    {
        Task<KycStatus> GetKycStatusAsync(string clientId);

        Task<IEnumerable<string>> GetClientsByStatus(KycStatus kycStatus);

        Task SetStatusAsync(string clientId, KycStatus status);
    }
}