using VolunTrack.DTO;
using VolunTrack.Enums;

namespace VolunTrack.Services
{
    public interface IUserService
    {
        Task<ToggleUserCategoryResult> ToggleUserCategory(int userId, int categoryId);
        Task<List<CategoryDto>> GetUserCategoriesAsync(int userId);
        Task<UserDto> SetUserRoleAsync(int userId, UserRole userRole);
    }
}
