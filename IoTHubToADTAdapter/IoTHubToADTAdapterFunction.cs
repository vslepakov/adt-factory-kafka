using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System;
using Azure.Identity;
using Azure.DigitalTwins.Core;
using System.Threading.Tasks;

namespace IoTHubToADTAdapter
{
    public static class IoTHubToADTAdapterFunction
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("IoTHubToADTAdapterFunction")]
        public static Task Run([IoTHubTrigger("messages/events", Connection = "IoTHubConnectionString")]EventData message, ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processes a message: {Encoding.UTF8.GetString(message.Body.Array)}");

            var adtInstanceUrl = Environment.GetEnvironmentVariable("AdtInstanceUrl");
            var credential = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

            var adtProcessor = new ADTDataProcessor(client, log);
            return adtProcessor.ProcessAsync(message);
        }
    }
}