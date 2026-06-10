namespace Monitoring.ViewModel
{
    public class EndpointGroupVM
    {
        public string Id { get; set; }
        public string GroupName { get; set; }
        public int? TotalDeviceCount { get; set; }
        public string GroupPatchScheduledTimeId { get; set; }
        public bool IsGroupPatchEnabled { get; set; }
        public DateTime? ScheduledTime { get; set; }
    }
}
