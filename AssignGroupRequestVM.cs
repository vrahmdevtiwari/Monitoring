namespace Monitoring.ViewModel
{
    public class AssignGroupRequestVM
    {
        public List<string> DeviceIds { get; set; }
        public string GroupName { get; set; }   // ← added
        public string OrgId {  get; set; }
    }

    
}
