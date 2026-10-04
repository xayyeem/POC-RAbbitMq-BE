using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class RabbitMqProducer
    {
        private readonly IConfiguration _configuration;

        public RabbitMqProducer(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task PublishAsync(ReportQueueMessage message)
        {
            var host = _configuration["RabbitMQ:Host"];
            var port = int.Parse(_configuration["RabbitMQ:Port"]!);
            var username = _configuration["RabbitMQ:UserName"];
            var password = _configuration["RabbitMQ:Password"];
            var queueName = _configuration["RabbitMQ:QueueName"];

            var factory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = username,
                Password = password
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var json = JsonSerializer.Serialize(message);

            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
        }
    }
}
