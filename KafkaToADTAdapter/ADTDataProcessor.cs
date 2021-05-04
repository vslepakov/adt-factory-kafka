using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KafkaToADTAdapter
{
    public class ADTDataProcessor : IKafkaDataProcessor
    {
        private readonly ILogger _logger;

        public string Id => "ADTDataProcessor";

        public ADTDataProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public Task ProcessAsync(Message<Ignore, string> message)
        {
            _logger.LogInformation("Handling ADT Update");

            return Task.CompletedTask;
        }
    }
}
