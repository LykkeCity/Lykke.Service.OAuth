﻿using System;
using System.Threading.Tasks;

namespace AzureStorage.Queue
{
    public class QueueData
    {
        public object Token { get; set; }
        public object Data { get; set; }
    }

    public class QueueType
    {
        public string Id { get; set; }
        public Type Type { get; set; }

        public static QueueType Create(string id, Type type)
        {
            return new QueueType
            {
                Id = id,
                Type = type
            };
        }
    }

    public interface IQueueExt
    {
        Task<string> PutMessageAsync(object itm);

        Task<QueueData> GetMessageAsync();
        Task FinishMessageAsync(QueueData token);

        Task<object[]> GetMessagesAsync(int maxCount);

        Task ClearAsync();

        void RegisterTypes(params QueueType[] type);
    }
}
