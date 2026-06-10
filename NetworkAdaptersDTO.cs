namespace Monitoring.Models.DTO
{
    public class NetworkAdaptersDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string InterfaceIndex { get; set; }
        public string Status { get; set; }
        public string MACAddress { get; set; }
        public string Speed { get; set; }
        public ActiveNetworkDetailDTO? ActiveNetworkDetails { get; set; } = null;
    }
}
