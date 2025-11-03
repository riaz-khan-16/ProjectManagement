using Microsoft.AspNetCore.Http.HttpResults;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;


namespace ProjectManagementAPI.Services
{
    public class EventPublisher: IEventPublisher
    {

        private readonly ConnectionFactory _factory;

        public EventPublisher(ConnectionFactory factory)
        {

            _factory = factory;
        }

        public async Task PublishEvent(string queueName, string message)

        {
            //Create a connection
            using var connection = await _factory.CreateConnectionAsync();

            // Create a channel (used to communicate with RabbitMQ)
            using var channel = await connection.CreateChannelAsync();

            // Declare a queue — ensures the queue exists before publishing
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Convert message to byte array
            var body = Encoding.UTF8.GetBytes(message);

            // Publish to the default exchange using the queue name as the routing key
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                body: body
            );

            Console.WriteLine($" [x] Sent '{message}' to queue '{queueName}'");



        }




    }


}
