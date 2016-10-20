using System;
using Common.Log;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAuth.EventHandler
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
            _errorLog.WriteError(context.Exception.Source, null, null, context.Exception, DateTime.UtcNow);
        }
    }
}