namespace BusinessService.Messages.Settings
{
    public class EmailGeneratorSettings
    {
        public bool EmailTemplatesRemote { get; set; }
        public string EmailTemplatesHost { get; set; }
        public int RefundTimeoutInDays { get; set; }
    }
}
