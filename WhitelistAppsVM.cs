namespace Monitoring.ViewModel
{
    public class WhitelistAppsVM
    {
        public string DeviceName { get; set; }
        public string LoginUser { get; set; }
        public string PublicIP { get; set; }
        public string? Status { get; set; } = string.Empty;
        public bool Restricted { get; set; } = false; // if app found in blocked list then return true or false
    }
}
