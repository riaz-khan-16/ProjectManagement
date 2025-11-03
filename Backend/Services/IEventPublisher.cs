using System.Threading.Tasks;
namespace ProjectManagementAPI.Services
{
    public interface IEventPublisher
    {
        Task PublishEvent(string queueName, string message);
        
    }
}
