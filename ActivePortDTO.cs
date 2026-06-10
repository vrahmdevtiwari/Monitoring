namespace Monitoring.Models.DTO
{
    public class ActivePortDTO
    {
        public string Proto { get; set; }
        public string LocalAddress { get; set; }
        public string ForeignAddress { get; set; }
        public string State { get; set; }
        public string PID { get; set; }
    }
}
