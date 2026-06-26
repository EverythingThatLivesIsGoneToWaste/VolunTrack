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

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Category>> GetAllActiveAsync() =>
            await _context.Categories.Where(c => c.IsActive).ToListAsync();

        public async Task<List<Category>> GetAllAsync() =>
            await _context.Categories.ToListAsync();

        public async Task<List<Category>> GetByIdsAsync(List<int> ids)
        {
            return await _context.Categories
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Categories
                .AnyAsync(c => EF.Functions.ILike(c.Name, name));
        }

        public async Task<bool> RelationsExistAsync(int categoryId)
        {
            return await _context.EventCategories.AnyAsync(ec => ec.CategoryId == categoryId) ||
                   await _context.UserCategories.AnyAsync(uc => uc.CategoryId == categoryId);
        }

        public async Task AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
