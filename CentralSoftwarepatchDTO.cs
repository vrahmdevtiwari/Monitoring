namespace Monitoring.Models.DTO
{
    public class CentralSoftwarepatchDTO
    {
        
        public string? Id { get; set; }

        // Unique internal update identifier
       
        public string UpdateId { get; set; }

        // Software Name (Chrome, SQL Server, Java, etc.)
      
        public string SoftwareName { get; set; }

        // Vendor (Microsoft, Oracle, Google, Adobe)
      
        public string? Vendor { get; set; }

        // Patch / Advisory / Bulletin Number

        public string? PatchNumber { get; set; }

        // Patch Description / Summary

        public string? PatchDescription { get; set; }

        // Security / Bug Fix / Feature Update

        public string? Classification { get; set; }

        // Critical / High / Medium / Low
    
        public string? Severity { get; set; }

        // Software Version
  
        public string? Version { get; set; }

        // 32-bit / 64-bit

        public string? BitRate { get; set; }

        // Windows / Linux / Mac

        public string? Platform { get; set; }

        // Build Number (Optional)

        public string? BuildNumber { get; set; }

        // File Size
 
        public string? Size { get; set; }

        // Related Articles / CVE references

        public string? Articles { get; set; }

        // Release Date

        public string? ReleaseDate { get; set; }
        public string? FileName { get; set; }
        // Physical file path

        public string? PatchPath { get; set; }

        // Product Family (Dropdown FK)

        public string? ProductFamily { get; set; }

        // Soft delete flag

        public bool IsDeleted { get; set; } = false;

        // Audit Fields

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
        public string? InstalledParaters { get; set; }
    }
}
