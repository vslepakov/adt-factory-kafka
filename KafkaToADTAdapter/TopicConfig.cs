using System;
using System.Collections.Generic;

namespace KafkaToADTAdapter
{
    public static class TopicConfig
    {
        public static string StatusTopic => Environment.GetEnvironmentVariable("ResourceStatusTopic");

        public static string KpiTopic => Environment.GetEnvironmentVariable("KpiTopic");

        public static IList<string> AllTopics => new List<string>
        {
            StatusTopic,
            KpiTopic
        };
    }
}
