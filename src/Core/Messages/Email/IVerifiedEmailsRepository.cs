using System.Threading.Tasks;

namespace Core.Messages.Email
{
    public interface IVerifiedEmailsRepository
    {
        Task AddOrReplaceAsync(string email);
        Task<bool> IsEmailVerified(string email);
    }
}
