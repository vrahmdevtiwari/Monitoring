namespace Monitoring.Models.DTO
{
    public class StorageVolumesDTO
    {
        public string BootVolume { get; set; }
        public string Capacity { get; set; }
        public string DriveLetter { get; set; }
        public string FileSystem { get; set; }
        public string FreeSpace { get; set; }
        public string Label { get; set; }
        public string SystemVolume { get; set; }
    }
}
