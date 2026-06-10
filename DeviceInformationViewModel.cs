using Monitoring.Models.DTO;
namespace Monitoring.ViewModel
{
    public class DeviceInformationViewModel
    {
        public LocationDTO? LocationDTO { get; set; }
        public EASpecificationsDTO? EASpecificationDTO { get; set; }
        public List<AppsDTO>? InstalledApps { get; set; }
        public List<PatchesDTO>? Updates { get; set; }
        public List<PortsDTO>? Ports { get; set; }
        public List<ServicesDTO>? Services { get; set; }
        public List<ProcessesDTO>? Processes { get; set; }
        public List<ProcessorDTO>? Processors { get; set; }
        public List<RAMDetailsDTO>? RAMDetails { get; set; }
        public List<ScheduledTasksDTO>? ScheduledTasks { get; set; }
        public List<StorageVolumesDTO>? StorageVolumes { get; set; }
        public List<GraphicCardsDTO>? GraphicCards { get; set; }
        public List<RAIDControllersDTO>? RAIDControllers { get; set; }
        public List<NetworkAdaptersDTO>? NetworkAdapters { get; set; }
        public List<PhysicalDrivesDTO>? PhysicalDrives { get; set; }
        public OtherSpecificationDTO? OtherSpecifications { get; set; }
        public List<AccountDTO>? Accounts { get; set; }
        public List<ActivePortDTO>? ActivePorts { get; set; }
        public List<ActiveNetworkDetailDTO>? ActiveNetworkDetails { get; set; }
        public ResourceUtilDTO? ResourceUtils { get; set; }
        public List<DiskDetailDTO>? DiskDetails { get; set; }
        public string ObjectID { get; set; }
        public string OrgID { get; set; }
    }
}
