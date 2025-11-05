using ProjectManagementAPI.Services;

public class RabbitMqHostedService : IHostedService
{
    private readonly IEventConsumer _consumer;

    public RabbitMqHostedService(IEventConsumer consumer)
    {
        _consumer = consumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Start the consumer in background
        _ = _consumer.StartAsync("Backend API Development", cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
