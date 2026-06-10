namespace Monitoring.Models
{
    public class ProductFamiliesVM
    {
        public int UniqueCode { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
