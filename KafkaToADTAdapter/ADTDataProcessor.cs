using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Azure.DigitalTwins.Core;
using Newtonsoft.Json;
using Azure;

namespace KafkaToADTAdapter
{
    public class ADTDataProcessor : IKafkaDataProcessor
    {
        private readonly DigitalTwinsClient _client;
        private readonly ILogger _logger;

        public string Id => "ADTDataProcessor";

        public ADTDataProcessor(DigitalTwinsClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task ProcessAsync(string topic, Message<Ignore, string> message)
        {
            _logger.LogInformation("Handling ADT Update");

            if(topic == TopicConfig.StatusTopic)
            {
                await HandleStatusMessageAsync(message.Value);
            }
            else if(topic == TopicConfig.KpiTopic)
            {
                await HandleKpiMessageAsync(message.Value);
            }
            else
            {
                _logger.LogWarning($"Received message on an unknown topic {topic}: {message.Value}");
            }
        }

        private async Task HandleStatusMessageAsync(string message)
        {
            try
            {
                var statusMessage = JsonConvert.DeserializeObject<StatusMessage>(message);
                await UpdateTwinPropertAsync(statusMessage.PrId, "/Status", (int)statusMessage.Status);
            }
            catch (JsonSerializationException)
            {
                _logger.LogError($"Failed to deserialize status message {message}");
            }
        }

        private async Task HandleKpiMessageAsync(string message)
        {
            try
            {
                var kpiMessage = JsonConvert.DeserializeObject<KpiMessage>(message);

                // We need to map a telemetry message to a Twin property and type
                var twinData = kpiMessage.ToTwinData();

                if (twinData == null)
                {
                    _logger.LogWarning($"Received unknown kpi metric {kpiMessage.KpiId}. No Twin mapping found!");
                    return;
                }

                await UpdateTwinPropertAsync(kpiMessage.PrId, $"/{twinData.TwinPropertyName}", twinData.TwinValue);
            }
            catch (JsonSerializationException)
            {
                _logger.LogError($"Failed to deserialize kpi message {message}");
            }
        }

        private async Task UpdateTwinPropertAsync(string twinId, string propertyPath, object value)
        {
            try
            {
                var patch = new JsonPatchDocument();
                patch.AppendReplace(propertyPath, value);
                await _client.UpdateDigitalTwinAsync(twinId, patch);
            }
            catch (RequestFailedException exc)
            {
                _logger.LogError($"*** Error:{exc.Status}/{exc.Message}");
            }
        }
    }
}
