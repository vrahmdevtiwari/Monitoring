namespace Monitoring.Models.DTO
{
    public class AvailablePatchesDTO
    {
        public string AssetId { get; set; }
        public List<FileUploadDTO> FileUploads { get; set; }
    }
}
