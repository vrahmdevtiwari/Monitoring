using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring.Data
{
    [Table("PasswordHistory")]
    public class PasswordHistory
    {
        [Key]
        public int HistoryID { get; set; }

        public string UserID { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        public DateTime DateUpdated { get; set; }

        [MaxLength(50)]
        public string? DeviceType { get; set; }

        public string? DeviceName { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(100)]
        public string? OperatingSystem { get; set; }

        [MaxLength(100)]
        public string? MobileBrand { get; set; }

        [MaxLength(100)]
        public string? MobileModel { get; set; }

        [MaxLength(50)]
        public string? MobileOS { get; set; }

        [MaxLength(15)]
        public string? MobileNumber { get; set; }

        [MaxLength(255)]
        public string? BrowserName { get; set; }

        [MaxLength(255)]
        public string? BrowserVersion { get; set; }

        [MaxLength(50)]
        public string? IPAddress { get; set; }

        public string? Location { get; set; }

        [MaxLength(256)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Applications { get; set; }

        [MaxLength(50)]
        public string? DeviceMemory { get; set; }

        [MaxLength(50)]
        public string? HardwareConcurrency { get; set; }

        [MaxLength(20)]
        public string? Language { get; set; }

        [MaxLength(100)]
        public string? OsManufacturer { get; set; }

        [MaxLength(100)]
        public string? OsName { get; set; }

        [MaxLength(100)]
        public string? OsVersion { get; set; }

        [MaxLength(100)]
        public string? Platform { get; set; }

        [MaxLength(50)]
        public string? ScreenResolution { get; set; }

        public string? UserAgent { get; set; }

        [MaxLength(50)]
        public string? Latitude { get; set; }

        [MaxLength(50)]
        public string? Longitude { get; set; }

        [MaxLength(255)]
        public string? Org { get; set; }

        [MaxLength(100)]
        public string? RegionName { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }
    }
}
