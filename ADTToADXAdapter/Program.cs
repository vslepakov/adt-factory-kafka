using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace ADTToADXAdapter
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

            var consumerConfig = new ConsumerConfig
            {
                BlobContainerName = Environment.GetEnvironmentVariable("BlobContainerName"),
                BlobStorageConnectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString"),
                EhConnectionString = Environment.GetEnvironmentVariable("InEhConnectionString"),
                EhName = Environment.GetEnvironmentVariable("InEhName")
            };

            var producerConfig = new ProducerConfig
            {
                EhConnectionString = Environment.GetEnvironmentVariable("OutEhConnectionString"),
                EhName = Environment.GetEnvironmentVariable("OutEhName")
            };

            await using var processor = new TwinUpdateProcessor(producerConfig, logger);
            var consumer = new TwinUpdateConsumer(consumerConfig, logger);
            consumer.AddProcessor(processor);

            await consumer.StartAsync(cts.Token);
            WhenCancelled(cts.Token).Wait();
        }

        private static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }
    }
}
