using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimulatedDevice
{
    public class Simulation
    {
        private readonly SimulationConfig _config;
        private readonly DeviceClient _deviceClient;
        private readonly ILogger _logger;

        public Simulation(SimulationConfig config, DeviceClient deviceClient, ILogger logger)
        {
            _config = config;
            _deviceClient = deviceClient;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting simulation...");

            await Task.WhenAll(
                SimulatedResourceKpiChangeAsync(cancellationToken),
                SimulateStatusChangeAsync(cancellationToken)
             );
        }

        private async Task SimulatedResourceKpiChangeAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var pr in _config.ProdResources)
                {
                    await SimulateKpisAsync(pr.PrId, _config.KpiSimulationConfigs.Where(config => config.PrType == pr.PrType));
                }

                await Task.Delay(_config.KpiChangeIntervalInSeconds * 1000);
            }
        }

        private async Task SimulateStatusChangeAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var pr in _config.ProdResources)
                {
                    var values = Enum.GetValues(typeof(ResourceStatus));
                    var random = new Random();

                    var statusMessage = new ProdResourceStatus
                    {
                        PrId = pr.PrId,
                        Status = (ResourceStatus)values.GetValue(random.Next(values.Length))
                    };

                    var payload = JsonConvert.SerializeObject(statusMessage);

                    using var message = new Message(Encoding.UTF8.GetBytes(payload))
                    {
                        ContentEncoding = "utf-8",
                        ContentType = "application/json",
                    };

                    message.Properties.Add("MessageType", _config.ResourceStatusMessageType);

                    _logger.LogInformation($"Publishing message of type {_config.ResourceStatusMessageType} to IoT Hub");
                    await _deviceClient.SendEventAsync(message);
                }

                await Task.Delay(_config.StatusChangeIntervalInSeconds * 1000);
            }
        }

        private async Task SimulateKpisAsync(string prId, IEnumerable<KpiSimulationConfig> kpiSimConfigs)
        {
            foreach (var kpiSimConfig in kpiSimConfigs)
            {
                var random = new Random();
                var kpiValue = random.NextDouble() * (kpiSimConfig.MaxValue - kpiSimConfig.MinValue) + kpiSimConfig.MinValue;

                var kpiMessage = new ProdResourceKpi
                {
                    PrId = prId,
                    KpiId = kpiSimConfig.KpiId,
                    Type = kpiSimConfig.KpiId,
                    Value = kpiValue
                };

                var payload = JsonConvert.SerializeObject(kpiMessage);

                using var message = new Message(Encoding.UTF8.GetBytes(payload))
                {
                    ContentEncoding = "utf-8",
                    ContentType = "application/json",
                };

                message.Properties.Add("MessageType", _config.ResourceKpiMessageType);

                _logger.LogInformation($"Publishing message of type {_config.ResourceKpiMessageType} to IoT Hub");
                await _deviceClient.SendEventAsync(message);
            }
        }
    }
}
