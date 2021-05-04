using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SimulatedDevice
{
    public class KafkaProducer
    {
        private readonly ILogger _logger;
        private readonly ProducerConfig _config;

        public KafkaProducer(ProducerConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task ProduceAsync(string topic, string message)
        {
            try
            {
                _logger.LogInformation($"Publishing message to Kafka on topic {topic}...");

                using var producer = new ProducerBuilder<Null, string>(_config).Build();
                var result = await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });

                _logger.LogInformation($"Publish result on topic {topic}: Status: {result.Status}, Message {result.Message}");
            }
            catch (ProduceException<string, string> e)
            {
                _logger.LogError($"Failed to deliver message: {e.Message} [{e.Error.Code}]");
            }
        }
    }
}
