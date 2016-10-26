using System;
using System.Threading.Tasks;

namespace Common.Log
{

    public interface ILog
    {
        Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null);
        Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null);
        Task WriteErrorAsync(string component, string process, string context, Exception exeption, DateTime? dateTime = null);

        Task WriteFatalErrorAsync(string component, string process, string context, Exception exeption, DateTime? dateTime = null);

        int Count { get; }
    }
}
