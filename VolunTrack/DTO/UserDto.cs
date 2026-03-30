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
            var dto = new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAtUtc = user.CreatedAtUtc,
                Categories = user.Categories?.Select(uc => CategoryDto.FromEntity(uc.Category)).ToList() ?? []
            };
            return dto;
        }

        public static string GetAvatarByRole(UserRole role)
        {
            return role switch
            {
                UserRole.Volunteer => "/images/avatars/volunteer.png",
                UserRole.EventCoordinator => "/images/avatars/coordinator.png",
                UserRole.RegionCoordinator => "/images/avatars/region-coordinator.png",
                UserRole.Administrator => "/images/avatars/administrator.png",
                _ => "/images/avatars/default.png"
            };
        }

        public static string GetRoleName(UserRole role)
        {
            return role switch
            {
                UserRole.Volunteer => "Волонтёр",
                UserRole.EventCoordinator => "Координатор мероприятий",
                UserRole.RegionCoordinator => "Региональный координатор",
                UserRole.Administrator => "Администратор",
                _ => "Неизвестно"
            };
        }
    }
}
