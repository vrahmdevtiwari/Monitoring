namespace Monitoring.Models.DTO
{
    public class PatchResponseDto
    {
        public List<CentralOSPatchesDTO>? OSPatches { get; set; }
        public List<CentralSoftwarepatchDTO>? SoftwarePatches { get; set; }
    }
}
