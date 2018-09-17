using Common.Log;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAuth.EventFilter
{
    public class UnhandledExceptionFilter : IExceptionFilter
    {
        private readonly ILog _errorLog;

        public UnhandledExceptionFilter(ILogFactory logFactory)
        {
            _errorLog = logFactory.CreateLog(this);
        }

        public void OnException(ExceptionContext context)
        {
            _errorLog.Error(context.Exception, context.Exception.Source);
        }
    }
}
