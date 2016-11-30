namespace BusinessService.Messages.Settings
{
    public class ServiceBusEmailSettings
    {
        public string NamespaceUrl { get; set; }
        public string PolicyName { get; set; }
        public string Key { get; set; }
        public string QueueName { get; set; }
    }
}
