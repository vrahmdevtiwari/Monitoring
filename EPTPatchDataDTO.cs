namespace Monitoring.Models.DTO
{
    public class EPTPatchDataDTO
    {
        public string PatchID { get; set; }
        public string PatchName { get; set; }
        public string? PatchOS { get; set; }
        public string? PatchBitRate { get; set; }
        public string KBNumber { get; set; }
        public string? KBNumberDescription { get; set; }
        public string? PatchStatus { get; set; }

    }
}
