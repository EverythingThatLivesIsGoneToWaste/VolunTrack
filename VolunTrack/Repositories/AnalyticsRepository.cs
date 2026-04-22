using Microsoft.EntityFrameworkCore;
using VolunTrack.Data;
using VolunTrack.Models;
using VolunTrack.Enums;

namespace VolunTrack.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Regular methods
        public async Task<Dictionary<string, decimal>> GetUserHoursStatsAsync(int userId)
        {
            return await _context.Participations
                .Where(p => p.UserId == userId)
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, TotalHours = g.Sum(p => p.TotalHours) })
                .ToDictionaryAsync(k => k.Status.ToString(), v => v.TotalHours);
        }

        public async Task<Category?> GetUserFavoriteCategoryAsync(int userId)
        {
            return await _context.Participations
                .Where(p => p.UserId == userId)
                .SelectMany(p => p.Event.EventCategories.Select(ec => ec.Category))
                .GroupBy(c => new { c.Id, c.Name, c.ColorRgb })
                .Select(g => new
                {
                    Category = new Category
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        ColorRgb = g.Key.ColorRgb
                    },
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Select(x => x.Category)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalConfirmedHoursAsync(int userId)
        {
            return await _context.Participations
                .Where(p => p.UserId == userId && p.Status == ParticipationStatus.Approved)
                .SumAsync(p => p.TotalHours);
        }

        // Universal methods
        public async Task<Dictionary<string, List<Event>>> GetUserEventsStatsAsync(int? userId = null)
        {
            throw new NotImplementedException();
        }

        // Admin methods
        public async Task<Dictionary<Event, int>> GetParticipantsCountByEventsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Event>> GetCompletedEventsByMonth()
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, decimal>> GetTotalHoursByCategoryAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetTotalActiveUsersAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, int>> GetUserRegistrationsByMonthAsync()
        {
            throw new NotImplementedException();
        }
    }
}
