using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.EventLogs
{
    public interface IAuthorizationLogRecord
    {
        string Email { get; set; }
        DateTime DateTime { get; set; }

        string UserAgent { get; set; }
    }

    public class AuthorizationLogRecord : IAuthorizationLogRecord
    {
        public AuthorizationLogRecord(string email, string userAgent)
        {
            Email = email;
            DateTime = DateTime.UtcNow;
            UserAgent = userAgent;
        }

        public string Email { get; set; }
        public DateTime DateTime { get; set; }
        public string UserAgent { get; set; }
    }

    public interface IAuthorizationLogsRepository
    {
        Task AddRecordAsync(IAuthorizationLogRecord record);

        Task<IEnumerable<IAuthorizationLogRecord>> GetAsync(string email, DateTime from, DateTime to);
    }
}
