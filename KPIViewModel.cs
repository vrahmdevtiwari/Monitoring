namespace Monitoring.ViewModel
{
    public class KPISViewModel
    {
        public string Total { get; set; } = "0";
        public string Failed { get; set; } = "0";
        public string Success { get; set; } = "0";
        public string AlreadyInstalled { get; set; } = "0";
        public string Missing { get; set; } = "0";
        public string ReportedDevices { get; set; } = "0";
        public string ApprovedDevices { get; set; } = "0";
        public string UnApprovedDevices { get; set; } = "0";
        public MissingPatchesSummary MissingPatches { get; set; }
    }
}
