namespace Monitoring.Models.DTO
{
    public class AgentManagerDeviceList
    {
        public string SystemName { get; set; }
        public string LoginUser { get; set; }
        public string Domain { get; set; }
        public string Privileges { get; set; }
        public string Manufacturer { get; set; }
        public string OS { get; set; }
        public string PublicIP { get; set; }
        public int ID { get; set; }
        public bool CanUpdate { get; set; }
        public string CurrentVersion { get; set; }
        public string LatestVersion { get; set; }
        public string OrgID { get; set; }
        public string ObjectId { get; set; }
    }
}
