namespace Monitoring.Models.DTO
{
    public class EADeviceDTO
    {
        public int Id { get; set; }

        public string AssetID { get; set; }

        public string Manufacturer { get; set; }

        public string MappedTo { get; set; }

        public string MappedSince { get; set; }

        public string AssetLocation { get; set; }

        public string Status { get; set; }

        public string DeviceType { get; set; }
    }
}
