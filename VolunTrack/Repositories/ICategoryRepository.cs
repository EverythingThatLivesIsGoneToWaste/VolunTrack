using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllActiveAsync();
        Task<List<Category>> GetAllAsync();

        Task<List<Category>> GetByIdsAsync(List<int> ids);
    }
}
