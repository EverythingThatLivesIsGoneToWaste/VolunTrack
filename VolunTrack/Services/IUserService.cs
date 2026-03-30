using VolunTrack.DTO;

namespace VolunTrack.Services
{
    public interface IUserService
    {
        Task<ToggleUserCategoryResult> ToggleUserCategory(int userId, int categoryId);
        Task<List<CategoryDto>> GetUserCategoriesAsync(int userId);
    }
}
