using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IClientSession
    {
        string ClientId { get; }
        string Token { get; }
        string ClientInfo { get; }
        DateTime Registered { get; }
        DateTime LastAction { get; }
    }

    public interface IClientsSessionsRepository
    {
        Task SaveAsync(string clientId, string token, string clientInfo);
        Task<IClientSession> GetAsync(string token);
        Task<IEnumerable<IClientSession>> GetByClientAsync(string clientId);

        Task UpdateClientInfoAsync(string clientId, string token, string clientInfo);

        Task DeleteSessionAsync(string clientId, string token);
    }

}
