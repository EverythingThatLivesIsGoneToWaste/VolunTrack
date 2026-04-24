using VolunTrack.Models;

namespace VolunTrack.DTO
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ColorRgb { get; set; }
        public bool IsActive { get; set; }

        public static CategoryDto? FromEntity(Category? category) =>
        category is null ? null : new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ColorRgb = category.ColorRgb,
            IsActive = category.IsActive,
        };
    }
}
