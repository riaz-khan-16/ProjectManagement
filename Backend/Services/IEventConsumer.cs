namespace ProjectManagementAPI.Services
{
    public interface IEventConsumer
    {
        Task StartListeningAsync(string queueName);  // The name of the queue to listen to
    }
}
