namespace Monitoring.Models.DTO
{
    public class EligiblePatches
    {
        public string SystemID { get; set; }
        public string OrgID { get; set; }
        public List<EPTPatchDataDTO> Patches { get; set; }
        public string DeviceName { get; set; }
        public string DeviceBIOS { get; set; }
        public string User { get; set; }

        public EligiblePatches()
        {
            Patches = new List<EPTPatchDataDTO>();
        }
    }

    
}
