using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaToADTAdapter
{
    public class KafkaConsumer
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, IKafkaDataProcessor> _dataProcessors;
        private readonly ConsumerConfig _config;

        public KafkaConsumer(ConsumerConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
            _dataProcessors = new ConcurrentDictionary<string, IKafkaDataProcessor>();
        }

        public async Task ConsumeAsync(IEnumerable<string> topics, CancellationToken cancellationToken)
        {
            using var consumer = 
                new ConsumerBuilder<Ignore, string>(_config)
                .SetErrorHandler((_, e) => _logger.LogError($"Error: {e.Reason}"))
                .Build();

            consumer.Subscribe(topics);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(cancellationToken);

                    if(consumeResult?.Message != null)
                    {
                        _logger.LogInformation($"Received message from Kafka topic {consumeResult.Topic}: {consumeResult.Message.Value}");

                        var tasks = new List<Task>();

                        foreach (var dataProcessor in _dataProcessors)
                        {
                            tasks.Add(dataProcessor.Value.ProcessAsync(consumeResult.Topic, consumeResult.Message));
                        }

                        await Task.WhenAll(tasks);
                    }
                }
            }
            catch (ConsumeException e)
            {
                _logger.LogError($"Consume error: {e.Error.Reason}");
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Kafka consumer was cancelled");
            }
            finally
            {
                consumer.Close();
            }
        }

        public void AddProcessor(IKafkaDataProcessor dataProcessor)
        {
            _dataProcessors.TryAdd(dataProcessor.Id, dataProcessor);
            _logger.LogInformation($"Added data processor with Id {dataProcessor.Id}");
        }

        public IKafkaDataProcessor RemoveProcessor(string Id)
        {
            _dataProcessors.TryRemove(Id, out IKafkaDataProcessor dataProcessor);
            _logger.LogInformation($"Removed data processor with Id {dataProcessor.Id}");

            return dataProcessor;
        }
    }
}
