using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ADTManager
{
    public class ADTModelUploader
    {
        private readonly DigitalTwinsClient _client;
        private readonly ILogger _logger;

        public ADTModelUploader(DigitalTwinsClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task UploadModelsAsync()
        {
            var appDir = Path.Combine(Directory.GetCurrentDirectory(), @"Models");

            var models = new List<string>();
            foreach (var file in Directory.EnumerateFiles(appDir, "*.json"))
            {
                var contents = File.ReadAllText(file);
                models.Add(contents);
            }

            var response = await _client.CreateModelsAsync(models);

            foreach (var model in response.Value)
            {
                _logger.LogInformation($"Model {model.Id} uploaded on {model.UploadedOn}");
            }
        }
    }
}
