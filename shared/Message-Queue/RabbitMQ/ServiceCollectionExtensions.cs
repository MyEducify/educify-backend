using Message_Queue.Queue;
using Message_Queue.RabbitMQ;
using MessageQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Message_Queue
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMqServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));

            // Register core services
            services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
            services.AddSingleton<IRabbitMqService, RabbitMqService>();
            services.AddSingleton<IQueueService, QueueService>();
            return services;
        }
    }
}
