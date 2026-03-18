using System.ComponentModel.DataAnnotations;
using VolunTrack.Enums;
using VolunTrack.Models;

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

        public static UserDto FromEntity(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAtUtc = user.CreatedAtUtc
            };
        }
    }
}
