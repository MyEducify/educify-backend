using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Message_Queue.Queue
{
    public interface IQueueService
    {
        void PublishMessage<T>(string queueName, T message, bool durable = true);
        void ConsumeMessage<T>(string queueName, Func<T, BasicDeliverEventArgs, IModel, Task> onMessageReceived);
    }

}
