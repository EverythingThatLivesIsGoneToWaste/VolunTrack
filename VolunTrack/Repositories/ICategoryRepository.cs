using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<List<Category>> GetAllActiveAsync();
        Task<List<Category>> GetAllAsync();

        Task<List<Category>> GetByIdsAsync(List<int> ids);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> RelationsExistAsync(int categoryId);

        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
    }
}
