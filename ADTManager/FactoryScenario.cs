using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ADTManager
{
    public class FactoryScenario
    {
        private const string FactoryTwinId = "Werk-2";
        private const string FactoryAreaTwinId = "Montage";
        private const string ProductionLineTwinId = "Linie-1";
        private const string ConveyorTwinId = "Band-15";
        private const string TransportTwinIdPrefix = "Transport-";
        private const int TransportCount = 3;
        private const string BufferTwinIdPrefix = "Puffer-";
        private const int BufferCount = 2;

        private readonly DigitalTwinsClient _client;
        private readonly ILogger _logger;

        public FactoryScenario(DigitalTwinsClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        // Quick and dirty cuz one timer
        public async Task RunAsync()
        {
            await CreateTransportAsync();
            await CreateBufferAsync();
            await CrateTransportToBufferRelationshipsAsync();
            await CreateConveyorAsync();
            await CreateProductionLineAsync();
            await CreateFactoryAreaLineAsync();
            await CreateFactoryAsync();
        }

        private async Task CreateTransportAsync()
        {
            _logger.LogInformation("Creating Transport Resources...");

            for (int i = 0; i < TransportCount; i++)
            {
                var twinData = new BasicDigitalTwin();
                twinData.Metadata.ModelId = "dtmi:com:contoso:factory:production_resource_engine_transport;1";
                var twinId = $"{TransportTwinIdPrefix}{i}";

                twinData.Contents.Add("PrId", twinId);
                twinData.Contents.Add("PrName", twinId);
                twinData.Contents.Add("Status", 1);
                twinData.Contents.Add("Speed", 10);
                twinData.Contents.Add("Vibration", 5);
                twinData.Id = twinId;

                await CreateOrReplaceTwinAsync(twinData);
            }
        }

        private async Task CreateBufferAsync()
        {
            _logger.LogInformation("Creating Buffer Resources...");

            for (int i = 0; i < BufferCount; i++)
            {
                var twinData = new BasicDigitalTwin();
                twinData.Metadata.ModelId = "dtmi:com:contoso:factory:production_resource_buffer_system;1";
                var twinId = $"{BufferTwinIdPrefix}{i}";

                twinData.Contents.Add("PrId", twinId);
                twinData.Contents.Add("PrName", twinId);
                twinData.Contents.Add("Status", 1);

                twinData.Contents.Add("BufferLevel", 15);
                twinData.Contents.Add("PieceCount", 50);
                twinData.Id = $"{BufferTwinIdPrefix}{i}";

                await CreateOrReplaceTwinAsync(twinData);
            }
        }

        private async Task CrateTransportToBufferRelationshipsAsync()
        {
            await CrateTransportToBufferRelationshipAsync($"{TransportTwinIdPrefix}0", $"{BufferTwinIdPrefix}0");
            await CrateTransportToBufferRelationshipAsync($"{BufferTwinIdPrefix}0", $"{TransportTwinIdPrefix}1");
            await CrateTransportToBufferRelationshipAsync($"{TransportTwinIdPrefix}1", $"{BufferTwinIdPrefix}1");
            await CrateTransportToBufferRelationshipAsync($"{BufferTwinIdPrefix}1", $"{TransportTwinIdPrefix}2");
        }

        private async Task CreateConveyorAsync()
        {
            _logger.LogInformation("Creating Conveyor...");

            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = "dtmi:com:contoso:factory:conveyor;1";

            twinData.Id = ConveyorTwinId;
            twinData.Contents.Add("ConveyorName", ConveyorTwinId);

            await CreateOrReplaceTwinAsync(twinData);

            await CreateConveyorToResourcesRelationship(TransportCount, TransportTwinIdPrefix);
            await CreateConveyorToResourcesRelationship(BufferCount, BufferTwinIdPrefix);
        }

        private async Task CreateProductionLineAsync()
        {
            _logger.LogInformation("Creating Production Line...");

            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = "dtmi:com:contoso:factory:production_line;1";
            twinData.Id = ProductionLineTwinId;

            twinData.Contents.Add("LineName", ProductionLineTwinId);
            twinData.Contents.Add("Efficiency", 50);
            twinData.Contents.Add("CurrentProductId", "1234");
            twinData.Contents.Add("LineOperationStatus", 1);

            await CreateOrReplaceTwinAsync(twinData);

            var relationship = new BasicRelationship
            {
                TargetId = ConveyorTwinId,
                Name = "contains_conveyor"
            };

            string relId = $"{ProductionLineTwinId}-contains_conveyor-{relationship.TargetId}";
            await CreateOrReplaceRelationshipAsync(ProductionLineTwinId, relId, relationship);
        }

        private async Task CreateFactoryAreaLineAsync()
        {
            _logger.LogInformation("Creating Factory Area...");

            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = "dtmi:com:contoso:factory:factory_area;1";
            twinData.Id = FactoryAreaTwinId;

            twinData.Contents.Add("AreaName", FactoryAreaTwinId);

            await CreateOrReplaceTwinAsync(twinData);

            var relationship = new BasicRelationship
            {
                TargetId = ProductionLineTwinId,
                Name = "has_production_line"
            };

            string relId = $"{FactoryAreaTwinId}-has_production_line-{relationship.TargetId}";
            await CreateOrReplaceRelationshipAsync(FactoryAreaTwinId, relId, relationship);
        }

        private async Task CreateFactoryAsync()
        {
            _logger.LogInformation("Creating Factory...");

            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = "dtmi:com:contoso:factory;1";
            twinData.Id = FactoryTwinId;

            twinData.Contents.Add("FactoryName", FactoryTwinId);
            twinData.Contents.Add("Country", "Germany");
            twinData.Contents.Add("ZipCode", "80809");
            twinData.Contents.Add("GeoLocation", new 
            {
                Latitude = 48.178716531624346,
                Longitude = 11.557705942784049
            });

            await CreateOrReplaceTwinAsync(twinData);

            var relationship = new BasicRelationship
            {
                TargetId = FactoryAreaTwinId,
                Name = "has_area"
            };

            string relId = $"{FactoryTwinId}-has_area-{relationship.TargetId}";
            await CreateOrReplaceRelationshipAsync(FactoryTwinId, relId, relationship);
        }

        private async Task CreateConveyorToResourcesRelationship(int resourceCount, string targetIdPrefix)
        {
            for (int i = 0; i < resourceCount; i++)
            {
                var relationship = new BasicRelationship
                {
                    TargetId = $"{targetIdPrefix}{i}",
                    Name = "contains_pr",
                };

                string relId = $"{ConveyorTwinId}-contains_pr-{relationship.TargetId}";
                await CreateOrReplaceRelationshipAsync(ConveyorTwinId, relId, relationship);
            }
        }

        private async Task CrateTransportToBufferRelationshipAsync(string srcId, string targetId)
        {
            var relationship = new BasicRelationship
            {
                TargetId = targetId,
                Name = "next_pr"
            };

            string relId = $"{srcId}-next_pr-{relationship.TargetId}";
            await CreateOrReplaceRelationshipAsync(srcId, relId, relationship);
        }

        private async Task CreateOrReplaceTwinAsync(BasicDigitalTwin twinData)
        {
            try
            {
                await _client.CreateOrReplaceDigitalTwinAsync(twinData.Id, twinData);
               _logger.LogInformation($"Created twin: {twinData.Id}");
            }
            catch (RequestFailedException e)
            {
                _logger.LogError($"Create twin error: {e.Status}: {e.Message}");
            }
        }

        private async Task CreateOrReplaceRelationshipAsync(string srcId, string relId, BasicRelationship relationship)
        {
            await _client.CreateOrReplaceRelationshipAsync(srcId, relId, relationship);
        }
    }
}
