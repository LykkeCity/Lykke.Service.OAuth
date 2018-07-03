namespace WebAuth.Models
{
    public class ResendCodeResult
    {
        public bool Result { get; set; }
        public bool IsCodeExpired { get; set; }

        public static ResendCodeResult Expired => new ResendCodeResult {IsCodeExpired = true};
    }
}
