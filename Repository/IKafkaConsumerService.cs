using System.Threading.Tasks;

namespace WebCar.Repository
{
    public interface IKafkaConsumerService
    {
        Task ConsumeMessagesAsync(CancellationToken cancellationToken);
    }
}