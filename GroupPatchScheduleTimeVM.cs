namespace Monitoring.ViewModel
{
    public class GroupPatchScheduleTimeVM
    {
        public string OrgId {  get; set; }
        public string GroupId { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public string GroupPatchScheduledTimeId { get; set; }
    }

    public class SaveScheduleRequestVM
    {
        public string GroupId { get; set; }
        public bool IsEnabled { get; set; }
        public string? ScheduledTime { get; set; }
        public string GpshistoryId { get; set; }
    }
}
