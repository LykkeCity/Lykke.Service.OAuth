using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebAuth.Tests.PasswordValidation
{
    class ThrowExceptionMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
