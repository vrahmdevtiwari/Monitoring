namespace Monitoring.Models.DTO
{
    public class ScheduledTasksDTO
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string NextRunTime { get; set; }
        public string LastRunTime { get; set; }
        public string LastRunResult { get; set; }
        public string Author { get; set; }
        public string Path { get; set; }
        public string Trigger { get; set; }
        public string CreatedDate { get; set; }
    }

}
