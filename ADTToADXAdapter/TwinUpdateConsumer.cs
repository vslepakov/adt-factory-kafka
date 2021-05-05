using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ADTToADXAdapter
{
    public class TwinUpdateConsumer
    {
        private readonly ConsumerConfig _config;
        private readonly ILogger _logger;
        private readonly EventProcessorClient _client;
        private readonly ConcurrentDictionary<string, ITwinUpdateProcessor> _dataProcessors;

        public TwinUpdateConsumer(ConsumerConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
            _dataProcessors = new ConcurrentDictionary<string, ITwinUpdateProcessor>();

            var storageClient = new BlobContainerClient(_config.BlobStorageConnectionString, _config.BlobContainerName);
            var consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            _client = new EventProcessorClient(storageClient, consumerGroup, _config.EhConnectionString, _config.EhName);

            _client.ProcessEventAsync += ProcessEventHandler;
            _client.ProcessErrorAsync += ProcessErrorHandler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client.StartProcessingAsync(cancellationToken);
        }

        public async Task StopAsync()
        {
            await _client.StopProcessingAsync();
        }

        public void AddProcessor(ITwinUpdateProcessor dataProcessor)
        {
            _dataProcessors.TryAdd(dataProcessor.Id, dataProcessor);
            _logger.LogInformation($"Added data processor with Id {dataProcessor.Id}");
        }

        public ITwinUpdateProcessor RemoveProcessor(string Id)
        {
            _dataProcessors.TryRemove(Id, out ITwinUpdateProcessor dataProcessor);
            _logger.LogInformation($"Removed data processor with Id {dataProcessor.Id}");

            return dataProcessor;
        }

        private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            _logger.LogInformation("Received Twin Update Event");
            var eventData = eventArgs.Data;

            var tasks = new List<Task>();

            foreach (var dataProcessor in _dataProcessors)
            {
                tasks.Add(dataProcessor.Value.ProcessAsync(eventData));
            }

            await Task.WhenAll(tasks);

            // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }

        private Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            // Write details about the error to the console window
            _logger.LogError($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            _logger.LogError(eventArgs.Exception.Message);
            return Task.CompletedTask;
        }
    }
}
