using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ProjectManagementAPI.Services
{
    public interface IEventConsumer
    {
        Task StartAsync(string queueName, CancellationToken cancellationToken = default);
    }

    public class EventConsumer : IEventConsumer
    {
        private readonly ConnectionFactory _factory;

        public EventConsumer()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost"
            };
        }

        public async Task StartAsync(string queueName, CancellationToken cancellationToken = default)
        {
            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync(); // or CreateChannelAsync if available

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            Console.WriteLine($" [*] Waiting for messages on queue: {queueName}");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received: {message}");

                // Here you can integrate SignalR to broadcast to clients:
                // await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

                await Task.Yield();
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumer: consumer);

            // Keep the consumer alive
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
