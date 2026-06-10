namespace Monitoring.Models.DTO
{
    public class OtherSpecificationDTO
    {
        public string CPUName { get; set; }
        public string OSVersion { get; set; }
        public string OSBuildVersion { get; set; } = "0";
        public string SystemUptime { get; set; }
        public string SystemModel { get; set; }
        public string SystemManufacturer { get; set; }
        public string SerialNumber { get; set; }
        public string InstalledRAM { get; set; }
        public string MACAAddress { get; set; }
        public string BIOSVersion { get; set; }
        public string Antivirus { get; set; }
    }
}
