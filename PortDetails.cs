namespace Monitoring.Models.DTO
{
    public class PortDetails
    {
        public string ProcessName { get; set; }

        public string Proto { get; set; }
        public string LocalAddress { get; set; }
        public string ForeignAddress { get; set; }
        public string State { get; set; }
        public string PID { get; set; }
    }
}
