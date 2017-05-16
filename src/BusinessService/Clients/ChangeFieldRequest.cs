namespace BusinessService.Clients
{
    public class ChangeFieldRequest
    {
        public string NewValue { get; private set; }

        public static ChangeFieldRequest Create(string newValue)
        {
            return new ChangeFieldRequest { NewValue = newValue };
        }
    }
}
