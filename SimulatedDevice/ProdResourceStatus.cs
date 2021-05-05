namespace SimulatedDevice
{
    public enum ResourceStatus
    {
        Ok = 1,
        Info = 2,
        Warning = 3,
        Error = 4
    }

    public class ProdResourceStatus
    {
        public string PrId { get; set; }

        public ResourceStatus Status { get; set; }
    }
}
