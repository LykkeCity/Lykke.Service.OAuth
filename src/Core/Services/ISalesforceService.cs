using Lykke.Service.Salesforce.Contract.Commands;

namespace Core.Services
{
    public interface ISalesforceService
    {
        void CreateContact(string email, string partnerId = null);
        void UpdateContact(UpdateContactCommand command);
    }
}
