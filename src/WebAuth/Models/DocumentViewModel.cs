using System.ComponentModel.DataAnnotations;
using Core.Kyc;
using Microsoft.AspNetCore.Http;
using WebAuth.Attributes;

namespace WebAuth.Models
{
    public class DocumentViewModel
    {
        public DocumentViewModel()
        {
        }

        public DocumentViewModel(string type)
        {
            DocumentType = type;
        }

        [DataType(DataType.Upload)]
        [FileTypes(KycDocumentTypes.AllDocumentExtensions, ErrorMessage = "Please choose .jpg, .jpeg or .png file")]
        public IFormFile Document { get; set; }

        public string DocumentType { get; set; }

        public string DocumentName { get; set; }

        public string DocumentMime { get; set; }
    }
}