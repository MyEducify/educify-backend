using Message_Queue.RabbitMQ;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

public class RabbitMqConnection : IRabbitMqConnection
{
    private readonly IConnection _connection;
    private readonly ConnectionFactory _factory;

    public RabbitMqConnection(IOptions<RabbitMqOptions> options)
    {
        _factory = new ConnectionFactory
        {
            HostName = options.Value.Host,
            Port = options.Value.Port,
            UserName = options.Value.Username,
            Password = options.Value.Password
        };
        _connection = _factory.CreateConnection();
    }

    public RabbitMQ.Client.IModel CreateChannel()
    {
        var channel = _connection.CreateModel();
        channel.ConfirmSelect(); // Enables publisher confirms
        return channel;
    }

    public void Dispose()
    {
        if (_connection.IsOpen)
            _connection.Close();
        _connection.Dispose();
    }
}
