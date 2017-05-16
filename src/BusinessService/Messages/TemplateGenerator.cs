using System;
using System.IO;
using System.Threading.Tasks;
using Core.Messages;
using Microsoft.AspNetCore.Hosting;

namespace BusinessService.Messages
{
    public class TemplateGenerator : ITemplateGenerator
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public TemplateGenerator(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<string> GenerateAsync<T>(string templateName, T templateModel, TemplateType type)
        {
            var templatesFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "Messages",
                type == TemplateType.Email ? "EmailTemplates" : "SmsTemplates");

            var path = Path.Combine(templatesFolder, templateName + ".mustache");

            try
            {
                return Nustache.Core.Render.FileToString(path, templateModel);
            }
            catch (InvalidCastException)
            {
                Console.WriteLine($"Incorrect model was passed for template: {path}");
                throw;
            }
        }
    }
}
