
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


namespace ProjectManagementAPI.Services
{
    public class EventConsumer: IEventConsumer
    {
        private readonly ConnectionFactory  _factory;

        public EventConsumer(ConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task StartListeningAsync(string queueName)
        {

            // 1️ Create connection
            using var connection = await _factory.CreateConnectionAsync();

            // 2️ Create channel
            using var channel = await connection.CreateChannelAsync();

            // 3️ Declare the same queue as your publisher
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // 4️ Create consumer
            var consumer = new AsyncEventingBasicConsumer(channel);

            // 5️ Handle messages
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($" [x] Received from queue '{queueName}': {message}");

                // You can later send this to SignalR or process further
                await Task.CompletedTask;
            };

            // 6️ Start consuming
            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumer: consumer
            );






        }




    }
}
