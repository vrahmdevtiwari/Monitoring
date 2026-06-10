namespace Monitoring.ViewModel
{
    public class AssignGroupByGroupIdVM
    {
        public List<string> DeviceIds { get; set; }
        public string GroupId { get; set; }   // ← added
        public string OrgId { get; set; }
    }
}
