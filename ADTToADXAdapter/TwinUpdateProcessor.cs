using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADTToADXAdapter
{
    public sealed class TwinUpdateProcessor : ITwinUpdateProcessor, IAsyncDisposable
    {
        private readonly ProducerConfig _config;
        private readonly ILogger _logger;
        private EventHubProducerClient _client;

        public string Id => "AdxProcessor";

        public TwinUpdateProcessor(ProducerConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;

            _client = new EventHubProducerClient(_config.EhConnectionString, _config.EhName);
        }

        public async Task ProcessAsync(EventData data)
        {
            _logger.LogInformation("Forwarding data to ADX EventHub...");

            var message = (JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data.EventBody));

            var adxUpdate = new Dictionary<string, object>();
            foreach (var operation in message["patch"])
            {
                if (operation["op"].ToString() == "replace" || operation["op"].ToString() == "add")
                {
                    string path = operation["path"].ToString().Substring(1);
                    path = path.Replace("/", ".");
                    adxUpdate.Add(path, operation["value"]);
                }
            }
            //Send an update if updates exist
            if (adxUpdate.Any())
            {
                adxUpdate.Add("$dtId", data.Properties["cloudEvents:subject"]);
                adxUpdate.Add("timestamp", DateTime.Now);

                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(adxUpdate));
                await _client.SendAsync(new[] { new EventData(bytes) });
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _client.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}
