using MessageQueue;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class RabbitMqService : IRabbitMqService
{
    private readonly IRabbitMqConnection _connection;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(IRabbitMqConnection connection, ILogger<RabbitMqService> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public void Publish<T>(string queueName, T message, bool durable = true)
    {
        using var channel = _connection.CreateChannel();
        channel.QueueDeclare(queue: queueName, durable: durable, exclusive: false, autoDelete: false);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }

    public void Consume<T>(string queueName, Action<T, BasicDeliverEventArgs, IModel> onMessageReceived)
    {
        var channel = _connection.CreateChannel();
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, args) =>
        {
            try
            {
                var body = args.Body.ToArray();
                var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));

                if (message != null)
                {
                    onMessageReceived(message, args, channel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing message");
                channel.BasicNack(args.DeliveryTag, false, requeue: true);
            }
        };

        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }

}
