namespace Monitoring.Models.DTO
{
    // Dev : Srikanth Erukulla - 02-02-2026
    public class OSPatches
    {
        public string SystemID { get; set; } = string.Empty;
        public string OrgID { get; set; } = string.Empty;

        public OSPatchesCount OSPatchesCount { get; set; }
        public List<EPTPatchDataDTO> AvailablePatches { get; set; }
        public List<InstalledPatchesDTO> InstalledPatches { get; set; }
        public List<InstalledAppDto> InstalledApps { get; set; }

        public string DeviceName { get; set; } = string.Empty;
        public string DeviceBIOS { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;

        public OSPatches()
        {
            AvailablePatches = new List<EPTPatchDataDTO>();
            InstalledPatches = new List<InstalledPatchesDTO>();
            InstalledApps= new List<InstalledAppDto>();
        }
    }
    public class OSPatchesCount
    {
        public int AvailablePatchesCount { get; set; }
        public int InstalledPatchesCount { get; set; }
    }
}
