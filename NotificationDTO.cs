using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring.Models.DTO
{
    public class NotificationDTO
    {
        public DateTime CreatedAt { get; set; }
        public string? User { get; set; }
        public string? Header { get; set; }
        public string? Body { get; set; }
        public bool IsRead { get; set; }
        public string? Message { get; set; }
        public string AssetID { get; set; }
        public string DevType { get; set; }
        public string OrgID { get; set; }
        public int Id { get; set; }
    }
}
