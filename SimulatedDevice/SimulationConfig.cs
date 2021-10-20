using System.Collections.Generic;

namespace SimulatedDevice
{
    public enum ProdResourceType
    {
        Buffer = 1,
        Transport = 2
    }

    public class ProdResource
    {
        public string PrId { get; set; }

        public ProdResourceType PrType { get; set; }
    }

    public class KpiSimulationConfig
    {
        public string KpiId { get; set; }

        public ProdResourceType PrType { get; set; }

        public double MinValue { get; set; }

        public double MaxValue { get; set; }
    }

    public class SimulationConfig
    {
        public string ResourceStatusMessageType { get; set; }

        public string ResourceKpiMessageType { get; set; }

        public int StatusChangeIntervalInSeconds { get; set; }

        public int KpiChangeIntervalInSeconds { get; set; }

        public IList<KpiSimulationConfig> KpiSimulationConfigs => new List<KpiSimulationConfig>
        {
            new KpiSimulationConfig{KpiId = "buffer_level", PrType = ProdResourceType.Buffer, MinValue = 0, MaxValue = 100 },
            new KpiSimulationConfig{KpiId = "piece_count", PrType = ProdResourceType.Buffer, MinValue = 0, MaxValue = 1000 },
            new KpiSimulationConfig{KpiId = "transport_speed", PrType = ProdResourceType.Transport, MinValue = 0, MaxValue = 50 },
            new KpiSimulationConfig{KpiId = "transport_vibration", PrType = ProdResourceType.Transport, MinValue = 0, MaxValue = 30 }
        };

        public IList<ProdResource> ProdResources => new List<ProdResource> 
        {
            new ProdResource { PrId = "Transport-0", PrType = ProdResourceType.Transport },
            new ProdResource { PrId = "Transport-1", PrType = ProdResourceType.Transport },
            new ProdResource { PrId = "Transport-2", PrType = ProdResourceType.Transport },
            new ProdResource { PrId = "Puffer-0", PrType = ProdResourceType.Buffer },
            new ProdResource { PrId = "Puffer-1", PrType = ProdResourceType.Buffer }
        };
    }
}
