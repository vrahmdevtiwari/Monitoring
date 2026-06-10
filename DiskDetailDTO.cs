using System.Text.Json.Serialization;

namespace Monitoring.Models.DTO
{
    public class DiskDetailDTO
    {
        public string Index { get; set; }
        public string DeviceID { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; } 
        public string MediaType { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareRevision { get; set; } 
        public string Capacity { get; set; }
        public string Partitions { get; set; } 
        public string InterfaceType { get; set; } 
        public string Status { get; set; }
        public string InstallDate { get; set; }
        [JsonPropertyName("partitionDetails")]
        public virtual List<PartitionDetailDTO> PartitionDetails { get; set; } 
    }
}
