using Core.VerificationCodes;

namespace WebAuth.Models
{
    public class VerificationCodeResult
    {
        public VerificationCode Code { get; set; }
        public bool IsEmailTaken { get; set; }
        public bool IsCodeExpired { get; set; }
    }
}
