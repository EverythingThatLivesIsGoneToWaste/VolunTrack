using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByLoginAsync(string login);

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
        Task AddUserCategoriesAsync(IEnumerable<UserCategory> userCategories);
        Task UpdateAsync(User user);
        Task RemoveAsync(User user);
    }
}
