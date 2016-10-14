using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Kyc
{
    public interface IKycDocument
    {
        string ClientId { get; }
        string DocumentId { get; }
        string Type { get; }
        string Mime { get; }

        string FileName { get; }
        DateTime DateTime { get; }
    }

    public class KycDocument : IKycDocument
    {
        public string ClientId { get; set; }
        public string DocumentId { get; set; }
        public string Type { get; set; }
        public string Mime { get; set; }
        public string FileName { get; set; }
        public DateTime DateTime { get; set; }

        public static KycDocument Create(string clientId, string type, string mime, string fileName)
        {
            return new KycDocument
            {
                ClientId = clientId,
                Type = type,
                Mime = mime,
                DateTime = DateTime.UtcNow,
                FileName = fileName
            };
        }
    }

    public interface IKycDocumentsRepository
    {
        Task<IKycDocument> AddAsync(IKycDocument kycDocument);
        Task<IEnumerable<IKycDocument>> GetAsync(string clientId);
        Task<IEnumerable<IKycDocument>> GetOneEachTypeAsync(string clientId);
        Task<IKycDocument> DeleteAsync(string clientId, string documentId);
    }

    public static class KycDocumentTypes
    {
        public const string IdCard = "IdCard";
        public const string ProofOfAddress = "ProofOfAddress";
        public const string Selfie = "Selfie";
        public const string BankAccount = "BankAccount";
        public const string ProofOfFunds = "ProofOfFunds";
        public const string AdditionalDocuments = "AdditionalDocuments";

        public const string AllDocumentExtensions = "jpg,jpeg,png";

        public static IEnumerable<string> GetAllTypes()
        {
            yield return IdCard;
            yield return ProofOfAddress;
            yield return Selfie;
        }

        public static bool HasDocumentType(this string type)
        {
            return GetAllTypes().FirstOrDefault(itm => itm == type) != null;
        }

        public static bool HasType(this IEnumerable<IKycDocument> documents, string type)
        {
            return documents.FirstOrDefault(itm => itm.Type == type) != null;
        }

        public static bool HasAllTypes(this IEnumerable<IKycDocument> documents)
        {
            var docs = documents as IKycDocument[] ?? documents.ToArray();

            return (docs.FirstOrDefault(itm => itm.Type == IdCard) != null)
                   && (docs.FirstOrDefault(itm => itm.Type == ProofOfAddress) != null)
                   && (docs.FirstOrDefault(itm => itm.Type == Selfie) != null);
        }

        public static string GetFileNameByType(this IEnumerable<IKycDocument> documents, string type)
        {
            var doc = documents.FirstOrDefault(itm => itm.Type.Equals(type));

            return doc?.FileName;
        }

        public static List<string> GetDocumentExtenstionList()
        {
            return new List<string>(AllDocumentExtensions.Split(','));
        }
    }
}