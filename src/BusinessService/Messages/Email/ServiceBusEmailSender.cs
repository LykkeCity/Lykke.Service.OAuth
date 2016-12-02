using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using BusinessService.Messages.Settings;
using Core.Messages.Email;
using System.Linq;
using Common;
using Common.Log;

namespace BusinessService.Messages.Email
{
    public class ServiceBusEmailSender : IEmailSender
    {
        private readonly ServiceBusEmailSettings _settings;
        private readonly ILog _log;

        public ServiceBusEmailSender(
            ServiceBusEmailSettings settings, 
            ILog log)
        {
            _settings = settings;
            _log = log;
        }
        public async Task SendEmailAsync(string email, EmailMessage emailMessage, string sender = null)
        {
            try
            {
                bool hasAttachments = emailMessage.Attachments != null && emailMessage.Attachments.Any();

                var message = new Message(emailMessage.Body)
                {
                    Properties = new Properties { MessageId = Guid.NewGuid().ToString() },
                    ApplicationProperties = new ApplicationProperties
                    {
                        ["email"] = email,
                        ["sender"] = !string.IsNullOrEmpty(sender) && sender.IsValidEmail() ? sender : string.Empty,
                        ["isHtml"] = emailMessage.IsHtml,
                        ["subject"] = emailMessage.Subject,
                        ["hasAttachment"] = hasAttachments
                    }
                };

                if (hasAttachments)
                {
                    message.ApplicationProperties["contentType"] = emailMessage.Attachments[0].ContentType;
                    message.ApplicationProperties["fileName"] = emailMessage.Attachments[0].FileName;

                    using (var ms = new MemoryStream())
                    {
                        emailMessage.Attachments[0].Stream.CopyTo(ms);
                        message.ApplicationProperties["file"] = ms.ToArray();
                    }
                }

                string policyName = WebUtility.UrlEncode(_settings.PolicyName);
                string key = WebUtility.UrlEncode(_settings.Key);
                string connectionString = $"amqps://{policyName}:{key}@{_settings.NamespaceUrl}/";

                var connection = await Connection.Factory.CreateAsync(new Address(connectionString));
                var amqpSession = new Session(connection);
                SenderLink senderLink = new SenderLink(amqpSession, "sender-link", _settings.QueueName);

                await senderLink.SendAsync(message);

                amqpSession.Close(0);
                connection.Close(0);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("EmailSender", "SendEmailAsync", string.Empty, ex);
            }
        }
    }
}
