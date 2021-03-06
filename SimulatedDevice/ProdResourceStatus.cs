namespace SimulatedDevice
{
    public enum ResourceStatus
    {
        Ok = 1,
        Warning = 2,
        Error = 3
    }

    public class ProdResourceStatus
    {
        public string PrId { get; set; }

        public ResourceStatus Status { get; set; }
    }
}
