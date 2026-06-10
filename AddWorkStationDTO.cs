using System;
using System.ComponentModel.DataAnnotations;

namespace Monitoring.Models.DTO
{
    public class AddWorkstationDTO
    {
        [Display(Name = "Device Type")]
        public string? deviceType { get; set; }
        public long? ObjectID { get; set; }

        [Required(ErrorMessage = "Please Enter Asset ID")]
        [Display(Name = "Asset ID")]
        public string AssetID { get; set; }

        [Required(ErrorMessage = "Please Select a Purchase Type")]
        [Display(Name = "Purchase Type")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid purchase type")]
        public int PurchaseType { get; set; }

        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? Fromdate { get; set; }

        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime? Todate { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [Required(ErrorMessage = "Please Select a Purchase Cost")]
        [Display(Name = "Purchase Cost")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Purchase cost must be a positive value")]
        public decimal? PurchaseCost { get; set; }

        [Display(Name = "Current Asset Value")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Current asset value must be a positive value")]
        public decimal? CurrentAssetVal { get; set; }

        [Display(Name = "Invoice Number")]
        public string? InvoiceNum { get; set; }

        [Display(Name = "Supplier")]
        public string? Supplier { get; set; }

        [Display(Name = "Supplier Warranty")]
        [Range(1, int.MaxValue, ErrorMessage = "Supplier warranty must be a positive value")]
        public int? SupplerWarranty { get; set; }

        [Display(Name = "Usage Status")]
        [Range(1, int.MaxValue, ErrorMessage = "Usage status must be a positive value")]
        public int? UsageStatus { get; set; }

        [Display(Name = "Asset Location")]
        public string? AssetLocation { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9:\-\s]+$", ErrorMessage = "Model can only contain letters, numbers, and hyphens.")]
        [Display(Name = "Manufacturer Name")]
        [Range(1, int.MaxValue, ErrorMessage = "Manufacturer name must be a positive value")]
        public int? ManufacturerName { get; set; }

        [Display(Name = "Model")]
        public string? Model { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9.:-]+$", ErrorMessage = "MACAddress can only contain letters, numbers, and hyphens.")]
        [Display(Name = "MAC Address")]
        public string? MacAddress { get; set; }

        [Display(Name = "Processor")]
        public string? Processor { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "RAM must be a positive value")]
        [Display(Name = "RAM")]
        public int? RAM { get; set; }

        [Display(Name = "Memory Type")]
        [Range(1, int.MaxValue, ErrorMessage = "Memory type must be a positive value")]
        public int? Memorytype { get; set; }

        [Display(Name = "Storage Type")]
        [Range(1, int.MaxValue, ErrorMessage = "Storage type must be a positive value")]
        public int? Storagetype { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Storage must be a positive value")]
        [Display(Name = "Storage")]
        public int? Storage { get; set; }

        [Display(Name = "Additional Information")]
        public string? AddlInfo { get; set; }

        public int? OrgID { get; set; }
        public string? Mapstatus { get; set; }

        [Display(Name = "Updated Time")]
        public DateTime? Updatedtime { get; set; }

        [Display(Name = "Updated User")]
        public string? UpdatedUser { get; set; }

        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [Display(Name = "BIOS Serial Number")]
        public string BIOS_SN { get; set; }

        [Display(Name = "Tracker ID")]
        public string? TrackerID { get; set; }

        [Display(Name = "Device Status")]
        [Range(1, int.MaxValue, ErrorMessage = "Device status must be a positive value")]
        public int? devstatus { get; set; }

        [Display(Name = "Operating System")]
        public string? OperatingSystem { get; set; }

        // Server Extra
        [Display(Name = "Number of Ports")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of ports must be a positive value")]
        public int? Ports { get; set; }

        [Display(Name = "RAID Level")]
        [Range(0, 10, ErrorMessage = "RAID level must be between 0 and 10")]
        public int? RAID { get; set; }
    }
}
