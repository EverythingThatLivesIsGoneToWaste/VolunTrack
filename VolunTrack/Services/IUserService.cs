using VolunTrack.DTO;
using VolunTrack.Enums;

namespace VolunTrack.Services
{
    public interface IUserService
    {
        Task<ToggleUserCategoryResult> ToggleUserCategory(int userId, int categoryId);
        Task<List<CategoryDto>> GetUserCategoriesAsync(int userId);
        Task<UserDto> SetUserRoleAsync(int userId, UserRole userRole);
        Task<List<UserDto>> GetUsersAsync(string? search);
        Task<UserDto> ToggleUserActivityAsync(int userId);
        Task<List<EventDto>> GetUpcomingEventsAsync(int userId);
        Task<List<EventDto>> GetCompletedEventsAsync(int userId);
    }
}
