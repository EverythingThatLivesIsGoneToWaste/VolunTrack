namespace VolunTrack.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ColorRgb { get; set; } = 0xADD2AA;
        public bool IsActive { get; set; } = true;

        public List<UserCategory> UserCategories { get; set; } = [];
        public List<EventCategory> EventCategories { get; set; } = [];
    }
}
