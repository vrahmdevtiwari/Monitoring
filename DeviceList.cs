namespace Monitoring.Models
{
    public class DeviceList
    {
        public string SystemName { get; set; }
        public string LoginUser { get; set; }
        public string Domain { get; set; }
        public string Privileges { get; set; }
        public string Manufacturer { get; set; }
        public string OS { get; set; }
        public string PublicIP { get; set; }
        public int ID { get; set; }
        public string InITAM { get; set; }
        public bool IsApproved { get; set; }
        public string Status { get; set; }
        public string? CheckITAM { get; set; }
        public string? DeviceType { get; set; }
        public string? AssetId { get; set; }
        public string? ObjectId { get; set; }
        public string? BIOS { get; set; } 
    }
}
