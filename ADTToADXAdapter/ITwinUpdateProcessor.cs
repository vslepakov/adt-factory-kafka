using Azure.Messaging.EventHubs;
using System.Threading.Tasks;

namespace ADTToADXAdapter
{
    public interface ITwinUpdateProcessor
    {
        string Id { get; }

        Task ProcessAsync(EventData data);
    }
}
