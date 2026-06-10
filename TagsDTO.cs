namespace Monitoring.Models.DTO
{
    public class TagsDTO
    {
        public int Id { get; set; }
        public string TagName { get; set; }
        public string TagDescription { get; set; }
        public string TagValue { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
