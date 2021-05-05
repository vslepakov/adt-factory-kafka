namespace KafkaToADTAdapter
{
    public enum ResourceStatus
    {
        Ok = 1,
        Info = 2,
        Warning = 3,
        Error = 4
    }

    public class StatusMessage
    {
        public string PrId { get; set; }

        public ResourceStatus Status { get; set; }
    }
}
