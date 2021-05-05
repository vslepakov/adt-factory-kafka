using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Azure.DigitalTwins.Core;
using Azure.Identity;
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

            var adtInstanceUrl = Environment.GetEnvironmentVariable("AdtInstanceUrl");
            var credential = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

            var kafkaConsumer = new KafkaConsumer(config, logger);
            kafkaConsumer.AddProcessor(new ADTDataProcessor(client, logger));
            await kafkaConsumer.ConsumeAsync(TopicConfig.AllTopics, cts.Token);
        }
    }
}
