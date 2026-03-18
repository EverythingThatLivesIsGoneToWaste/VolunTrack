using Microsoft.EntityFrameworkCore;
using VolunTrack.Data;
using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllActiveAsync() =>
            await _context.Categories.Where(c => c.IsActive).ToListAsync();

        public async Task<List<Category>> GetAllAsync() =>
            await _context.Categories.ToListAsync();
    }
}
