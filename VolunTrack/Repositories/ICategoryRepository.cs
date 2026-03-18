using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllActiveAsync();
        Task<List<Category>> GetAllAsync();
    }
}
