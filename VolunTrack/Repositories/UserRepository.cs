using Microsoft.EntityFrameworkCore;
using VolunTrack.Data;
using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Categories)
                    .ThenInclude(uc => uc.Category)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByLoginAsync(string login)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Categories)
                    .ThenInclude(uc => uc.Category)
                .Include(u => u.Participations)
                .ToListAsync();
        }

        public async Task<List<User>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<UserCategory?> GetUserCategoryAsync(int userId, int categoryId)
        {
            return await _context.UserCategories.FirstOrDefaultAsync(
                uc => uc.UserId == userId && uc.CategoryId == categoryId);
        }

        public async Task<List<Category>> GetUserCategoriesAsync(int userId)
        {
            return await _context.UserCategories
                .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Category)
                .Select(uc => uc.Category)
                .ToListAsync();
        }

        public async Task<List<User>> SearchAsync(
            string? searchTerm = null, 
            UserRole? role = null, 
            bool? isActive = null, 
            int page = 1, 
            int pageSize = 10)
        {
            var query = _context.Users
                .Include(u => u.Categories)
                    .ThenInclude(uc => uc.Category)
                .Include(u => u.Participations)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    EF.Functions.ILike(u.Login, $"%{searchTerm}%") ||
                    EF.Functions.ILike(u.FullName, $"%{searchTerm}%") ||
                    EF.Functions.ILike(u.Email, $"%{searchTerm}%") ||
                    EF.Functions.ILike(u.Phone, $"%{searchTerm}%"));
            }

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var users = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return users;
        }

        public async Task<bool> ExistsByLoginAsync(string login)
        {
            return await _context.Users.AnyAsync(u => u.Login == login);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task AddUserCategoryAsync(UserCategory userCategory)
        {
            await _context.UserCategories.AddAsync(userCategory);
            await _context.SaveChangesAsync();
        }

        public async Task AddUserCategoriesAsync(IEnumerable<UserCategory> userCategories)
        {
            await _context.UserCategories.AddRangeAsync(userCategories);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserCategoryAsync(UserCategory userCategory)
        {
            _context.UserCategories.Remove(userCategory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsLeaderOfEventAsync(int userId, int eventId)
        {
            return await _context.UserLeaderAssignments
                .AnyAsync(ula => ula.UserId == userId && ula.EventId == eventId);
        }

        public async Task<List<UserReportDto>> GetUsersForReportAsync(bool? isActive, DateOnly? fromDate, DateOnly? toDate)
        {
            var query = _context.Users.AsQueryable();

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            if (fromDate.HasValue)
            {
                var fromUtc = fromDate.Value.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
                query = query.Where(u => u.CreatedAtUtc.Date >= fromUtc);
            }

            if (toDate.HasValue)
            {
                var toUtc = toDate.Value.ToDateTime(TimeOnly.MaxValue).ToUniversalTime();
                query = query.Where(u => u.CreatedAtUtc.Date <= toUtc);
            }

            var users = await query
                .Select(u => new UserReportDto
                {
                    Id = u.Id,
                    Login = u.Login,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    RegisteredAt = u.CreatedAtUtc,
                    TotalConfirmedHours = u.Participations
                    .Where(p => p.Status == ParticipationStatus.Approved)
                    .Sum(p => p.TotalHours),
                    CompletedEventsCount = u.Participations
                    .Where(p => p.Status == ParticipationStatus.Approved)
                    .Select(p => p.EventId)
                    .Distinct()
                    .Count(),
                })
                .ToListAsync();

            return users;
        }
    }
}
