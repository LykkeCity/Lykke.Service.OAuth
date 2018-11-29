using System;
using Common.Log;
using Core.Services;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.Salesforce.Contract;
using Lykke.Service.Salesforce.Contract.Commands;

namespace Lykke.Service.OAuth.Services
{
    public class SalesforceService : ISalesforceService
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ILog _log;

        public SalesforceService(
            ICqrsEngine cqrsEngine,
            ILogFactory logFactory
            )
        {
            _cqrsEngine = cqrsEngine;
            _log = logFactory.CreateLog(this);
        }
        
        public void CreateContact(string email, string partnerId = null)
        {
            try
            {
                _cqrsEngine.SendCommand(new CreateContactCommand
                {
                    Email = email,
                    //TODO: send partnerId once implemented
                    PartnerId = partnerId
                }, "oauth", SalesforceBoundedContext.Name);
            }
            catch (Exception ex)
            {
                _log.Error(nameof(CreateContact), ex);
            }
        }
    }
}
