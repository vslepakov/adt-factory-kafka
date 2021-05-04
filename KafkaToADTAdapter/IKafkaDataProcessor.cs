using Confluent.Kafka;
using System.Threading.Tasks;

namespace KafkaToADTAdapter
{
    public interface IKafkaDataProcessor
    {
        string Id { get; }

        Task ProcessAsync(Message<Ignore, string> message);
    }
}
