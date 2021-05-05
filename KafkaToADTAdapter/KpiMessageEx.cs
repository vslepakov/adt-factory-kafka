using System.Collections.Generic;

namespace KafkaToADTAdapter
{
    public class TwinData
    {
        public TwinData(string propertyName, object value)
        {
            TwinValue = value;
            TwinPropertyName = propertyName;
        }

        public string TwinPropertyName { get;}

        public object TwinValue { get; }
    }

    public static class KpiMessageEx
    {
        private const string IntegerType = "integer";
        private const string DoubleType = "double";

        private static IDictionary<string, (string, string)> PropertyMap = new Dictionary<string, (string, string)>
        {
            {"buffer_level", ("BufferLevel", IntegerType)},
            {"piece_count", ("PieceCount", IntegerType)},
            {"transport_speed", ("Speed", DoubleType)},
            {"transport_vibration", ("Vibration", DoubleType)}
        };

        public static TwinData ToTwinData(this KpiMessage kpiMessage)
        {
            if(PropertyMap.TryGetValue(kpiMessage.KpiId, out (string, string) mappingInfo))
            {
                if(mappingInfo.Item2 == IntegerType)
                {
                    return new TwinData(mappingInfo.Item1, (int)kpiMessage.Value);
                }
                else
                {
                    return new TwinData(mappingInfo.Item1, kpiMessage.Value);
                }
            }

            return null;
        }
    }
}
