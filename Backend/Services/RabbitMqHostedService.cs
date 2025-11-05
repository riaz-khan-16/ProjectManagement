using Microsoft.Extensions.Hosting;

namespace ProjectManagementAPI.Services
{
    public class RabbitMqHostedService : IHostedService
    {
        private readonly IEventConsumer _consumer;

        public RabbitMqHostedService(IEventConsumer consumer)
        {
            _consumer = consumer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Start consuming messages in the background
            _ = _consumer.StartAsync("Backend API Development", cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Nothing special needed, cancellation token will stop StartAsync loop
            return Task.CompletedTask;
        }
    }
}
