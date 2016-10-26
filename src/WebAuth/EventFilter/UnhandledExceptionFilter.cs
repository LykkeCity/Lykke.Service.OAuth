using Common.Extenstions;
using Common.Log;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAuth.EventFilter
{
    public class UnhandledExceptionFilter : IExceptionFilter
    {
        private readonly ILog _errorLog;

        public UnhandledExceptionFilter(ILog errorLog)
        {
            _errorLog = errorLog;
        }

        public void OnException(ExceptionContext context)
        {
            _errorLog.WriteErrorAsync(context.Exception.Source, null, null, context.Exception).RunSync();
        }
    }
}