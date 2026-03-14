using System.ComponentModel.DataAnnotations;
using VolunTrack.Enums;

namespace VolunTrack.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        [Required]
        public List<CategoryDto> Categories { get; set; } = [];
    }
}
