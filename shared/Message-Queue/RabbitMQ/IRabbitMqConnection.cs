using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;

public interface IRabbitMqConnection : IDisposable
{
    RabbitMQ.Client.IModel CreateChannel();
}
