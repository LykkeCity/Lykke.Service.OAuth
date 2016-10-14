using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.EventLogs
{
    public interface IRequestsLogRecord
    {
        DateTime DateTime { get; set; }
        string Url { get; set; }
        string Request { get; set; }
        string Response { get; set; }
        string UserAgent { get; set; }
    }

    public interface IRequestsLogRepository
    {
        Task WriteAsync(string clientId, string url, string request, string response, string userAgent);
        Task<IEnumerable<IRequestsLogRecord>> GetRecords(string clientId, DateTime from, DateTime to);
    }
}
