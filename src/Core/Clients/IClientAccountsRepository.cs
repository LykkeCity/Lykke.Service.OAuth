using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IClientAccount
    {
        DateTime Registered { get; }
        string Id { get; }
        string Email { get; }
        string PartnerId { get; }
        string Phone { get; }
        string Pin { get; }
        string NotificationsId { get; }
    }

    public class ClientAccount : IClientAccount
    {
        public DateTime Registered { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Pin { get; set; }
        public string NotificationsId { get; set; }

        public string PartnerId { get; set; }

        public static ClientAccount Create(string email, string phone, string partnerId)
        {
            return new ClientAccount
            {
                Email = email,
                Registered = DateTime.UtcNow,
                Phone = phone,
                PartnerId = partnerId
            };
        }
    }

    public interface IClientAccountsRepository
    {
        Task<IClientAccount> RegisterAsync(IClientAccount clientAccount, string password);
        Task<bool> IsTraderWithEmailExistsAsync(string email, string partnerId = null);
        Task<IClientAccount> AuthenticateAsync(string email, string password, string partnerId = null);
        Task ChangePassword(string clientId, string newPassword);
        Task ChangePhoneAsync(string clientId, string phoneNumber);
        Task<IClientAccount> GetByIdAsync(string id);
        Task<IEnumerable<IClientAccount>> GetByIdAsync(string[] ids);
        Task<IClientAccount> GetByEmailAndPartnerIdAsync(string email, string partnerId = null);
        Task<IEnumerable<IClientAccount>> GetByEmailAsync(string email);
        Task<string> GenerateNotificationsId(string clientId);
        Task<bool> IsPasswordCorrect(string clientId, string password);
        Task SetPin(string clientId, string newPin);
    }

}
