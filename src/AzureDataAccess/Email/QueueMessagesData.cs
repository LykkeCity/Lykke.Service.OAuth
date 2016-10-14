using Core.Broadcast;

namespace AzureDataAccess.Email
{
    public class SendEmailData<T>
    {
        public string EmailAddress { get; set; }
        public T MessageData { get; set; }


        public static SendEmailData<T> Create(string emailAddress, T msgData)
        {
            return new SendEmailData<T>
            {
                EmailAddress = emailAddress,
                MessageData = msgData
            };
        }
    }

    public class SendBroadcastData<T>
    {
        public BroadcastGroup BroadcastGroup { get; set; }
        public T MessageData { get; set; }


        public static SendBroadcastData<T> Create(BroadcastGroup broadcastGroup, T msgData)
        {
            return new SendBroadcastData<T>
            {
                BroadcastGroup = broadcastGroup,
                MessageData = msgData
            };
        }
    }
}
