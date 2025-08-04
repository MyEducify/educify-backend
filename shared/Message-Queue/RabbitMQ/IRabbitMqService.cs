using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageQueue;

public interface IRabbitMqService
{
    void Publish<T>(string queueName, T message, bool durable = true);
    void Consume<T>(string queueName, Action<T, BasicDeliverEventArgs, IModel> onMessageReceived);

}
