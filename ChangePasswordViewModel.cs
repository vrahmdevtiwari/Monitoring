using System.ComponentModel.DataAnnotations;

namespace Monitoring.ViewModel
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Please Enter Current Password")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "Please Enter New Password")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Please Enter Confirm Password")]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
        public SystemInfo? System { get; set; }
    }
    public class SystemInfo
    {
        public string? BrowserName { get; set; }
        public string? BrowserVersion { get; set; }
        public string? DeviceMemory { get; set; }
        public string? DeviceType { get; set; }
        public string? HardwareConcurrency { get; set; }
        public string? Language { get; set; }
        public string? OsManufacturer { get; set; }
        public string? OsName { get; set; }
        public string? OsVersion { get; set; }
        public string? Platform { get; set; }
        public string? ScreenResolution { get; set; }
        public string? UserAgent { get; set; }
        public string? IP { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Org { get; set; }
        public string? region { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
