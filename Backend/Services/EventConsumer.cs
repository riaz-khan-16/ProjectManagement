using Microsoft.AspNetCore.SignalR;
using ProjectManagementAPI.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ProjectManagementAPI.Services
{   
    
    public interface IEventConsumer
    {
        // Method to start listening to a specific queue asynchronously.
        // The CancellationToken allows graceful shutdown when the app stops.
        Task StartAsync(string queueName, CancellationToken cancellationToken = default);
    }

    // Implements the RabbitMQ consumer that listens for messages from a queue.
    public class EventConsumer : IEventConsumer
    {
        // A RabbitMQ connection factory object — used to create connections to the broker.
        private readonly ConnectionFactory _factory;
        private readonly IHubContext<NotificationHub> _hubContext;

        // Constructor: initializes the connection factory with hostname details.
        public EventConsumer(ConnectionFactory factory, IHubContext<NotificationHub> hubContext)
        {
            _factory = factory;
            _hubContext = hubContext;
        }

        // Starts consuming messages from the specified queue.
        public async Task StartAsync(string queueName, CancellationToken cancellationToken = default)
        {
            // Creates a connection to the RabbitMQ broker asynchronously.
             var connection = await _factory.CreateConnectionAsync();

            // Opens a communication channel within the connection (a virtual session).
            var channel = await connection.CreateChannelAsync(); // or CreateModelAsync in older versions.

            // Declares a queue to ensure it exists before consuming.
            // If the queue already exists, this call is safe and simply verifies its properties.
            await channel.QueueDeclareAsync(
                queue: queueName,      // Queue name to listen to.
                durable: false,        // The queue won’t survive a broker restart.
                exclusive: false,      // The queue can be used by multiple connections.
                autoDelete: false,     // The queue won’t be deleted when the last consumer disconnects.
                arguments: null);      // No extra arguments.

            Console.WriteLine($" [*] Waiting for messages on queue: {queueName}");

            // Creates an asynchronous consumer that can handle messages
            var consumer = new AsyncEventingBasicConsumer(channel);

            // Subscribes to the "ReceivedAsync" event — triggered whenever a message arrives.
            consumer.ReceivedAsync += async (model, ea) =>
            {
                
                var body = ea.Body.ToArray(); // Extracts the message body (binary data).
                var message = Encoding.UTF8.GetString(body); // Converts the byte array to a human-readable UTF-8 string.
                Console.WriteLine($" [x] Received: {message}");  // Prints the received message to the console.
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // Broadcast the received message to all connected SignalR clients
                await Task.Yield();  // Returns control back to the event loop (good async practice).
            };

            // Tells RabbitMQ to start delivering messages from the queue to this consumer.
            await channel.BasicConsumeAsync(
                queue: queueName,      // The name of the queue to consume from.
                autoAck: true,         // Automatically acknowledge messages (no manual confirmation).
                consumer: consumer);   // The consumer object handling messages.
                                       
            await Task.Delay(-1, cancellationToken); // Keep the consumer alive indefinitely

        }
    }
}
