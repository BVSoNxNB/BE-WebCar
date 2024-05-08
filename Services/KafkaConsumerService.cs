using Confluent.Kafka;
using Microsoft.Extensions.Options;
using WebCar.Models;

public class KafkaConsumerService : IHostedService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly ConsumerConfig _config;
    private IConsumer<string, string> _consumer;

    public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IOptions<KafkaSettings> options)
    {
        _logger = logger;
        _config = new ConsumerConfig
        {
            BootstrapServers = options.Value.Hosts,
            SecurityProtocol = SecurityProtocol.SaslPlaintext,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = options.Value.Username,
            SaslPassword = options.Value.Password,
            GroupId = "webCar-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Kafka consumer service is starting...");
        _consumer = new ConsumerBuilder<string, string>(_config).Build();
        _consumer.Subscribe("WebCar");

        var consumeTask = Task.Run(() => ConsumeMessagesAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Close();
        return Task.CompletedTask;
    }

    private async Task ConsumeMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(cancellationToken);
                if (consumeResult?.Message != null)
                {
                    _logger.LogInformation($"Received message: {consumeResult.Message.Value}");
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogError($"Error occurred: {ex.Error.Reason}");
            }
        }
    }
}