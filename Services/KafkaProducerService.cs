using Confluent.Kafka;
using Microsoft.Extensions.Options;
using WebCar.Models;

public class KafkaProducerService
{
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly ProducerConfig _config;
    private IProducer<string, string> _producer;

    public KafkaProducerService(ILogger<KafkaProducerService> logger, IOptions<KafkaSettings> options)
    {
        _logger = logger;
        _config = new ProducerConfig
        {
            BootstrapServers = options.Value.Hosts,
            SecurityProtocol = SecurityProtocol.SaslPlaintext,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = options.Value.Username,
            SaslPassword = options.Value.Password
        };
        _producer = new ProducerBuilder<string, string>(_config).Build();
    }

    public async Task ProduceMessageAsync(string topic, string message)
    {
        try
        {
            var deliveryResult = await _producer.ProduceAsync(topic, new Message<string, string> { Value = message });
            _logger.LogInformation($"Delivered message to {deliveryResult.TopicPartitionOffset}");
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError($"Failed to deliver message: {ex.Message}");
        }
    }
}