using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Partner
{
    public interface IPartnerClientAccount
    {
        string PublicId { get; set; }
        string ClientId { get; set; }
        DateTime Created { get; set; }
    }

    public class PartnerClientAccount : IPartnerClientAccount
    {
        public string ClientId { get; set; }
        public DateTime Created { get; set; }
        public string PublicId { get; set; }
    }

    public interface IPartnerClientAccountRepository
    {
        Task RegisterAsync(IPartnerClientAccount partner, string password);
        Task<bool> IsTraderRegisteredForPartnerAsync(string clientId, string publicId);
        Task<IPartnerClientAccount> AuthenticateAsync(string clientId, string publicId, string password);
        Task ChangePassword(string clientId, string publicId, string newPassword);
        Task<IEnumerable<IPartnerClientAccount>> GetForPartnerAsync(string publicId);
        Task<IEnumerable<IPartnerClientAccount>> GetForClientAsync(string clientId);
    }
}
