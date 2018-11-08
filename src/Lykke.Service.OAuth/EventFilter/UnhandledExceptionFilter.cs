using Common.Log;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lykke.Service.OAuth.EventFilter
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
            if (context.Exception is LykkeApiErrorException)
                return;

            _errorLog.Error(context.Exception, context.Exception.Source);
        }
    }
}
