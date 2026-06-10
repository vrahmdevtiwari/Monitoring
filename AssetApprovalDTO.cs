namespace Monitoring.Models.DTO
{
    public class AssetApprovalDTO
    {
        public int ID { get; set; }
        public string AssetID { get; set; }
        public string OrgID { get; set; }
        public string ObjectID { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
