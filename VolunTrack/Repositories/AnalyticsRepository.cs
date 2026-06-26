using Microsoft.EntityFrameworkCore;
using VolunTrack.Data;
using VolunTrack.Models;
using VolunTrack.Enums;
using VolunTrack.DTO;

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
                .GroupBy(c => new { c.Id, c.Name, c.Description, c.ColorRgb, c.IsActive })
                .Select(g => new
                {
                    Category = new Category
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        Description = g.Key.Description,
                        ColorRgb = g.Key.ColorRgb,
                        IsActive = g.Key.IsActive
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

        public async Task<int> GetUserTotalCompletedEventsAsync(int userId)
        {
            return await _context.Participations
                .Where(p => p.UserId == userId && p.Event.Status == EventStatus.Completed && p.Status == ParticipationStatus.Approved)
                .Select(p => p.EventId)
                .Distinct()
                .CountAsync();
        }

        // Universal methods
        public async Task<Dictionary<string, List<Event>>> GetUserEventsStatsAsync(int? userId = null)
        {
            var query = _context.Participations
                .Where(p => p.Event.Status == EventStatus.Completed 
                    && p.Status == ParticipationStatus.Approved)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(p => p.UserId == userId.Value);

            var result = await query
                .SelectMany(p => p.Event.EventCategories.Select(ec => new
                {
                    CategoryName = ec.Category.Name,
                    p.Event
                }))
                .Distinct()
                .GroupBy(x => x.CategoryName)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Events = g.Select(x => x.Event).ToList()
                })
                .ToDictionaryAsync(k => k.CategoryName, v => v.Events);

            return result;
        }

        // Admin methods
        public async Task<Dictionary<Event, int>> GetParticipantsCountByEventsAsync()
        {
            return await _context.Events
                .Where(e => e.Status == EventStatus.Completed)
                .Select(e => new
                {
                    Event = e,
                    Participations = e.Participations.Count()
                })
                .ToDictionaryAsync(k => k.Event, v => v.Participations);
        }

        public async Task<Dictionary<int, int>> GetCompletedEventsByMonthAsync(int year)
        {
            return await _context.Events
                .Where(e => e.Status == EventStatus.Completed && e.EndDateTime.Year == year)
                .GroupBy(e => e.EndDateTime.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Events = g.Count()
                })
                .ToDictionaryAsync(k => k.Month, v => v.Events);
        }

        public async Task<Dictionary<Category, decimal>> GetTotalHoursByCategoryAsync()
        {
            return await _context.Participations
                .Where(p => p.Event.Status == EventStatus.Completed && p.Status == ParticipationStatus.Approved)
                .SelectMany(p => p.Event.EventCategories.Select(ec => new
                {
                    ec.Category,
                    p.TotalHours
                }))
                .GroupBy(x => x.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalHours = g.Sum(x => x.TotalHours)
                })
                .ToDictionaryAsync(k => k.Category, v => v.TotalHours);
        }

        public async Task<int> GetTotalActiveUsersAsync()
        {
            return await _context.Users.Where(u => u.IsActive == true).CountAsync();
        }

        public async Task<Dictionary<int, int>> GetUserRegistrationsByMonthAsync(int year)
        {
            return await _context.Users
                .Where(u => u.IsActive && u.CreatedAtUtc.Year == year)
                .GroupBy(u => u.CreatedAtUtc.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Registrations = g.Count()
                })
                .ToDictionaryAsync(k => k.Month, v => v.Registrations);
        }

        public async Task<int> GetTotalCompletedEventsAsync()
        {
            return await _context.Events
                .Where(e => e.Status == EventStatus.Completed)
                .CountAsync();
        }
    }
}
