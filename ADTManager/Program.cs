using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ADTManager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Program>();

            var adtInstanceUrl = Environment.GetEnvironmentVariable("AdtInstanceUrl");
            var credential = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

            var modelUploader = new ADTModelUploader(client, logger);
            var scenario = new FactoryScenario(client, logger);

            await modelUploader.UploadModelsAsync();
            await scenario.RunAsync();
        }
    }
}
