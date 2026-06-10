using System.ComponentModel.DataAnnotations;

namespace Monitoring.Models.DTO
{
    public class InstalledPatchesDTO
    {
        public int Id { get; set; }
        public string AssetId { get; set; }
        public string Patch { get; set; }
        public string Title { get; set; }
        [MaxLength(256)] public string Description { get; set; }
        public string InstalledOn { get; set; }
        public string Version { get; set; }
        public string OrgId { get; set; }

    }

    public class InstalledPatches
    {
        public string AssetId { get; set; }
        public List<InstalledPatchesDTO> PatchesDTOs { get; set; }
    }
}
