
using ProjectManagementAPI.Services;

// This class implements the IHostedService interface, meaning it defines a background service
// that starts automatically when the application starts and stops when the application shuts down.
public class RabbitMqHostedService : IHostedService
{
    private readonly IEventConsumer _consumer;
    public RabbitMqHostedService(IEventConsumer consumer)
    {
        _consumer = consumer;  // Store the injected consumer instance in the private field.
    }

    // This method is called automatically when the application starts.
    // It begins the background task that listens for RabbitMQ messages.
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // The underscore (_) means we're intentionally ignoring the returned Task.
        // This line starts the consumer asynchronously in the background, without waiting for it to finish.
        // "Backend API Development" is the name of the queue the consumer will listen to.
        _ = _consumer.StartAsync("Backend API Development", cancellationToken);

        // Return a completed task immediately because we don’t want to block the application startup.
        return Task.CompletedTask;
    }

    // This method is called automatically when the application is shutting down.
    // Here you can clean up resources, stop background tasks, etc.
    // Currently, it just returns a completed task since no special cleanup is needed.
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
