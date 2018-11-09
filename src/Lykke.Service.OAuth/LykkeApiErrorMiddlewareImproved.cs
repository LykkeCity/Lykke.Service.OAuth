using System;
using System.Threading.Tasks;
using Common.Log;
using Core;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lykke.Service.OAuth
{
    public class LykkeApiErrorMiddlewareImproved
    {
        private readonly RequestDelegate _next;
        private readonly ILog _log;
        private readonly ExceptionsHandlingConfiguration _errorCodeConfiguration;

        public LykkeApiErrorMiddlewareImproved(
            RequestDelegate next,
            ExceptionsHandlingConfiguration errorCodeConfiguration,
            ILogFactory logFactory)
        {
            _next = next;
            _errorCodeConfiguration = errorCodeConfiguration;
            _log = logFactory.CreateLog(this);
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var handlerConfig = _errorCodeConfiguration.FindConfig(ex.GetType());

                if (handlerConfig != null)
                {
                    LogException(ex, handlerConfig.LogLevel);

                    await CreateErrorResponse(context, handlerConfig, ex);
                }
                else
                {
                    _log.Error(ex, ex.Source);

                    throw;
                }
            }
        }

        private void LogException(Exception ex, LogLevel logLevel)
        {
            string exContext = ex.CollectContext();

            switch (logLevel)
            {
                case LogLevel.Warning:
                    _log.Warning(ex.Message, ex, exContext);
                    break;
                case LogLevel.Error:
                    _log.Error(ex, ex.Message, exContext);
                    break;
                case LogLevel.Information:
                    _log.Info(message: ex.Message, exContext, ex);
                    break;
                case LogLevel.Critical:
                    _log.Critical(ex, ex.Message, exContext);
                    break;
                case LogLevel.None:
                    break;
            }
        }

        private async Task CreateErrorResponse(HttpContext ctx,
            ExceptionsHandlingConfiguration.HandlerConfig handlerConfig, Exception ex)
        {
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = (int) handlerConfig.ResponseStatusCode;
            await ctx.Response.WriteAsync(JsonConvert.SerializeObject(
                new LykkeApiErrorResponse
                {
                    Error = handlerConfig.ResponseErrorCode.Name,
                    Message = handlerConfig.ResponseErrorCode.DefaultMessage ?? ex.Message
                }));
        }
    }
}
