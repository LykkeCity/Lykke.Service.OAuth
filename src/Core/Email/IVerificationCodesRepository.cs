using System.Threading.Tasks;

namespace Core.Email
{
    public interface IVerificationCode
    {
        string Code { get; }
        string Key { get; }
        string Email { get; }
        int ResendCount { get; }
        string Referer { get; }
        string ReturnUrl { get; }
        string Cid { get; }
        string Traffic { get; }
    }

    public interface IVerificationCodesRepository
    {
        Task<IVerificationCode> AddCodeAsync(string email, string referer, string returnUrl, string cid, string traffic);
        Task<IVerificationCode> GetCodeAsync(string key);
        Task<IVerificationCode> UpdateCodeAsync(string key);
        Task DeleteCodesAsync(string email);
    }
}
