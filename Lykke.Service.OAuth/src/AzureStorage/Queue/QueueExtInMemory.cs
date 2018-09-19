using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureStorage.Queue
{
    public class QueueExtInMemory : IQueueExt
    {
        private readonly Queue<object> _queue = new Queue<object>();

        public Task<string> PutMessageAsync(object itm)
        {
            PutMessage(itm);
            return Task.FromResult("");
        }

        public Task<QueueData> GetMessageAsync()
        {
            var item = GetMessage();

            QueueData queueData = null;
            if (item != null)
                queueData = new QueueData
                {
                    Data = item,
                    Token = item
                };

            return Task.FromResult(queueData);
        }

        public Task FinishMessageAsync(QueueData token)
        {
            return Task.FromResult(0);
        }

        public Task<object[]> GetMessagesAsync(int maxCount)
        {
            var result = GetMessages(maxCount);
            return Task.FromResult(result);
        }

        public Task ClearAsync()
        {
            lock (_queue)
            {
                return Task.Run(() => _queue.Clear());
            }
        }

        public void RegisterTypes(params QueueType[] type)
        {
        }

        public void PutMessage(object itm)
        {
            lock (_queue)
                _queue.Enqueue(itm);
        }

        public object GetMessage()
        {
            lock (_queue)
            {
                if (_queue.Any())
                {
                    return _queue.Dequeue();
                }
            }
            return null;
        }

        public object[] GetMessages(int maxCount)
        {
            var result = new List<object>();
            lock (_queue)
            {
                while (result.Count < maxCount)
                {
                    if (_queue.Count == 0)
                        break;
                    result.Add(_queue.Dequeue());
                }
            }

            return result.ToArray();
        }
    }
}