using System;
using System.Collections.Generic;

namespace IoTHubToADTAdapter
{
    public static class MessageTypes
    {
        public static string Status => Environment.GetEnvironmentVariable("ResourceStatusMessageType");

        public static string Kpi => Environment.GetEnvironmentVariable("KpiMessageType");

        public static IList<string> All => new List<string>
        {
            Status,
            Kpi
        };
    }
}
