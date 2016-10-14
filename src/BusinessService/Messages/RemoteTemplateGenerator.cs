using System;
using System.Text;
using System.Threading.Tasks;
using Common.HttpRemoteRequests;
using Core.Messages;
using System.Reflection;
using BusinessService.Messages.Settings;

namespace BusinessService.Messages
{
    public class RemoteTemplateGenerator : ITemplateGenerator
    {
        private readonly EmailGeneratorSettings _emailGeneratorSettings;
        private readonly HttpRequestClient _httpRequestClient;

        public RemoteTemplateGenerator(EmailGeneratorSettings emailGeneratorSettings,
            HttpRequestClient httpRequestClient)
        {
            _emailGeneratorSettings = emailGeneratorSettings;
            _httpRequestClient = httpRequestClient;
        }

        public async Task<string> GenerateAsync<T>(string templateName, T templateVm, TemplateType type)
        {
            if (type == TemplateType.Sms)
                throw new ArgumentException("SMS template should be local!");

            var baseUri = new Uri(_emailGeneratorSettings.EmailTemplatesHost);
            var templateUri = new Uri(baseUri, templateName + ".html");

            var emailTemplate = GetEmailTemplate(templateUri.AbsoluteUri);

            var emailTemplateWithData = InsertData(await emailTemplate, templateVm);

            return emailTemplateWithData;
        }

        private async Task<string> GetEmailTemplate(string emailTemplateUri)
        {
            return await _httpRequestClient.GetRequest(emailTemplateUri);
        }

        private string InsertData<T>(string emailTemplate, T templateVm)
        {
            var sb = new StringBuilder(emailTemplate);

            foreach (var prop in templateVm.GetType().GetProperties())
                if (prop.GetValue(templateVm, null) != null)
                    sb.Replace("@[" + prop.Name + "]", prop.GetValue(templateVm, null).ToString());

            return sb.ToString();
        }
    }
}