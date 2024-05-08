namespace WebCar.Repository
{
    public interface IKafkaProducerService
    {
        Task ProduceMessageAsync(object message);
    }
}
