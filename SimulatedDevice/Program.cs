using Microsoft.Azure.Devices.Client;
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

            var deviceConnectionString = Environment.GetEnvironmentVariable("IoTHubConnectionString");

            var simulationConfig = new SimulationConfig
            {
                KpiChangeIntervalInSeconds = int.Parse(Environment.GetEnvironmentVariable("KpiChangeIntervalInSeconds")),
                StatusChangeIntervalInSeconds = int.Parse(Environment.GetEnvironmentVariable("StatusChangeIntervalInSeconds")),
                ResourceKpiMessageType = Environment.GetEnvironmentVariable("KpiMessageType"),
                ResourceStatusMessageType = Environment.GetEnvironmentVariable("ResourceStatusMessageType")
            };


            using var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);
            var simulation = new Simulation(simulationConfig, deviceClient, logger);
            await simulation.RunAsync(cts.Token);
        }
    }
}
