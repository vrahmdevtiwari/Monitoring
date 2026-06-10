using System.ComponentModel.DataAnnotations;

namespace Monitoring.Models.DTO
{
    // UPDATES
    public class PatchesDTO
    {
        public string Patch { get; set; }
        public string Title { get; set; }
        [MaxLength(256)]
        public string Description { get; set; }
        public string InstalledOn { get; set; }
        public string Version { get; set; }
    }
}
