using MessageQueue;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Message_Queue.Queue
{
    public class QueueService : IQueueService
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<QueueService> _logger;

        public QueueService(IRabbitMqService rabbitMqService, ILogger<QueueService> logger)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }
        public void PublishMessage<T>(string queueName, T message, bool durable = true)
        {
            _rabbitMqService.Publish(queueName, message, durable);
            _logger.LogInformation("Published message to {QueueName}: {@Message}", queueName, message);
        }
        public void ConsumeMessage<T>(string queueName, Func<T, BasicDeliverEventArgs, IModel, Task> onMessageReceived)
        {
            _rabbitMqService.Consume<T>(queueName, async (message, args, channel) =>
            {
                _logger.LogInformation("Received message from {QueueName}: {@Message}", queueName, message);
                await onMessageReceived(message, args, channel);
            });
        }
    }
}
