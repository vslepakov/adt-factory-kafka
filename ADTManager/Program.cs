using Azure.DigitalTwins.Core;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ADTManager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var adtInstanceUrl = Environment.GetEnvironmentVariable("AdtInstanceUrl");

            var credential = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

            await UploadModelsAsync(client);
        }

        private static async Task UploadModelsAsync(DigitalTwinsClient client)
        {
            var appDir = Path.Combine(Directory.GetCurrentDirectory(), @"Models");

            var models = new List<string>();
            foreach (var file in Directory.EnumerateFiles(appDir, "*.json"))
            {
                var contents = File.ReadAllText(file);
                models.Add(contents);
            }

            var response = await client.CreateModelsAsync(models);

            foreach (var model in response.Value)
            {
                Console.WriteLine($"Model {model.Id} uploaded on {model.UploadedOn}");
            }
        }
    }
}
