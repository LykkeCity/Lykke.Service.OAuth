namespace Core.Services
{
    public interface ISalesforceService
    {
        void CreateContact(string email, string partnerId = null);
    }
}
