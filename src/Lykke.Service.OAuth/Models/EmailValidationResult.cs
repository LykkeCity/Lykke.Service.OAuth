namespace Lykke.Service.OAuth.Models
{
    /// <summary>
    /// Email validation result
    /// </summary>
    public class EmailValidationResult
    {
        /// <summary>
        /// Indicates whether email has already been taken
        /// </summary>
        public bool IsEmailTaken { get; set; }
    }
}
