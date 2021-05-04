using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace KafkaToADTAdapter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Program>();

            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();

            var config = new ConsumerConfig
            {
                BootstrapServers = Environment.GetEnvironmentVariable("BootstrapServers"),
                GroupId = Environment.GetEnvironmentVariable("GroupId"),
                AutoOffsetReset = AutoOffsetReset.Earliest,
                BrokerAddressFamily = BrokerAddressFamily.V4,
                EnableAutoCommit = true
            };

            var topics = Environment.GetEnvironmentVariable("Topics").Split(',');

            var kafkaConsumer = new KafkaConsumer(config, logger);
            kafkaConsumer.AddProcessor(new ADTDataProcessor(logger));
            await kafkaConsumer.ConsumeAsync(topics, cts.Token);
        }
    }
}
