using Core.Email;

namespace WebAuth.Models
{
    public class VerificationCodeResult
    {
        public IVerificationCode Code { get; set; }
        public bool IsEmailTaken { get; set; }
    }
}
