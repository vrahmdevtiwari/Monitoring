namespace Monitoring.Models.DTO
{
    public class ResourceUtilDTO
    {
        public float CPUUsage { get; set; }
        public float PhysicalDiskUsage { get; set; }
        public float MemoryUsage { get; set; }
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
        public float GPUUsage { get; set; }

    }
}
