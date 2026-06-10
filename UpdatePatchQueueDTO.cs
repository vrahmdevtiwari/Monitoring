using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring.Models.DTO
{
    public class UpdatePatchQueueDTO
    {
        public int ID { get; set; }
        public string SystemID { get; set; }
        public string UpdateName { get; set; }
        public string UpdateKBNumber { get; set; }
        public int Status { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
