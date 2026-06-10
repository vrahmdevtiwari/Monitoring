namespace Monitoring.Models.DTO
{
    public class RAIDControllersDTO
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string PNPDeviceID { get; set; }
        public string Manufacturer { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string SystemCreationClassName { get; set; }
        public string SystemName { get; set; }
        public string ConfigManagerErrorCode { get; set; }
        public string ConfigManagerUserConfig { get; set; }
    }
}
