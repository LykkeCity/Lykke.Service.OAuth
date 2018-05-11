using System;
using MessagePack;

namespace Core.VerificationCodes
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class VerificationCode
    {
        public string Code { get; set; }
        public string Key { get; set; }
        public string Email { get; set; }
        public int ResendCount { get; set; }
        public string Referer { get; set; }
        public string ReturnUrl { get; set; }
        public string Cid { get; set; }
        public string Traffic { get; set; }

        public VerificationCode (string email, string referer, string returnUrl, string cid, string traffic)
        {
            Code = GenerateCode();
            Key = Guid.NewGuid().ToString("N");
            Email = email;
            Referer = referer;
            ReturnUrl = returnUrl;
            Cid = cid;
            Traffic = traffic;
        }

        public void UpdateCode()
        {
            Code = GenerateCode();
            ResendCount++;
        }
        
        private static string GenerateCode()
        {
            var rand = new Random(DateTime.UtcNow.Millisecond);
            return rand.Next(999999).ToString(new string('0', 6));
        }
    }
}
