using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace SimulatedDevice
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

            var config = new ProducerConfig
            {
                BootstrapServers = Environment.GetEnvironmentVariable("BootstrapServers"),
                ClientId = Environment.GetEnvironmentVariable("ClientId")
            };

            var topic = Environment.GetEnvironmentVariable("Topic");

            var kafkaProducer = new KafkaProducer(config, logger);
            await kafkaProducer.ProduceAsync(topic, "Hello Kafka! First Message");
        }
    }
}
