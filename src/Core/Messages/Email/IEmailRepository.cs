using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Messages.Email
{
    public interface ISmtpMailMock
    {
        string Id { get; }

        string Address { get; }

        DateTime DateTime { get; }

        string Subject { get; }

        string Body { get; }

        bool IsHtml { get; }
    }

    public interface IEmailMockRepository
    {
        Task<ISmtpMailMock> InsertAsync(string address, EmailMessage msg);
        Task<IEnumerable<ISmtpMailMock>> GetAllAsync();
        Task<IEnumerable<ISmtpMailMock>> Get(string email);
        Task<ISmtpMailMock> GetAsync(string email, string id);
    }
}
