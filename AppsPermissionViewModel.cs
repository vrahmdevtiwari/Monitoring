namespace Monitoring.ViewModel
{
    public class AppsPermissionViewModel
    {
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; } = string.Empty;
        public bool ApplicationPermission { get; set; } = false;
        public int OrganizationId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icons { get; set; } = string.Empty;
    }
}
