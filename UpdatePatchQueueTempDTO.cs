namespace Monitoring.Models
{
    public class UpdatePatchQueueTempDTO
    {
        public string SystemID { get; set; }
        public string PatchID { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public string OrgId { get; set; }
    }
}
