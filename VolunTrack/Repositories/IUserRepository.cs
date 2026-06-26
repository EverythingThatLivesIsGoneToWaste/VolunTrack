using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByLoginAsync(string login);
        Task<List<User>> GetAllAsync();
        Task<List<User>> GetByIdsAsync(IEnumerable<int> ids);
        Task<UserCategory?> GetUserCategoryAsync(int userId, int categoryId);
        Task<List<Category>> GetUserCategoriesAsync(int id);

        Task<List<User>> SearchAsync(
            string? searchTerm = null,
            UserRole? role = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20
        );

        Task<bool> ExistsByLoginAsync(string login);
        Task<bool> ExistsByEmailAsync(string email);

        Task AddAsync(User user);
        Task AddUserCategoryAsync(UserCategory userCategory);
        Task AddUserCategoriesAsync(IEnumerable<UserCategory> userCategories);
        Task RemoveUserCategoryAsync(UserCategory userCategory);
        Task UpdateAsync(User user);
        Task RemoveAsync(User user);

        Task<bool> IsLeaderOfEventAsync(int userId, int eventId);

        Task<List<UserReportDto>> GetUsersForReportAsync(bool? isActive, DateOnly? fromDate, DateOnly? toDate);
    }
}
