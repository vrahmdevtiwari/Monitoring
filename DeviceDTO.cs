namespace Monitoring.Models.DTO
{
    public class DeviceDTO
    {
        public string SystemName { get; set; }
        public string LoginUser { get; set; }
        public string Domain { get; set; }
        public string Privileges { get; set; }
        public string Manufacturer { get; set; }
        public string OS { get; set; }
        public string PublicIP { get; set; }
        public string ID { get; set; }
        public bool InITAM { get; set; }
        public bool IsApproved { get; set; }
        public string? OrgID { get; set; }
        public string? BIOS { get; set; } 
        public DateTime? LastSyncDate { get; set; }
        public DateTime? Created { get; set; }
    }
}
