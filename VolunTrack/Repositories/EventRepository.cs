using Microsoft.EntityFrameworkCore;
using VolunTrack.Data;
using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Event?> GetByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.EventCategories)
                    .ThenInclude(ec => ec.Category)
                .Include(e => e.EventPhotos)
                .Include(e => e.Participations)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Event>> GetAllAsync()
        {
            return await _context.Events
                .Include(e => e.EventCategories)
                    .ThenInclude(ec => ec.Category)
                .ToListAsync();
        }

        public async Task<List<Event>> GetUpcomingAsync()
        {
            return await _context.Events
                .Include(e => e.EventCategories)
                    .ThenInclude(ec => ec.Category)
                .Include(e => e.EventPhotos)
                .Include(e => e.Participations)
                .Where(e => e.StartDateTime > DateTime.UtcNow && e.Status == EventStatus.Published)
                .ToListAsync();
        }

        public async Task<List<Event>> GetByCategoryIdsAsync(List<int> categoryIds)
        {
            return await _context.Events
                .Include(e => e.EventCategories)
                    .ThenInclude(ec => ec.Category)
                .Include(e => e.EventPhotos)
                .Where(e => e.EventCategories
                    .Any(ec => categoryIds.Contains(ec.CategoryId)))
                .ToListAsync();
        }

        public async Task<List<Event>> GetByCoordinatorIdAsync(int coordinatorId)
        {
            return await _context.Events
                .Include(e => e.Participations)
                .Where(e => e.CreatedByUserId == coordinatorId).ToListAsync();
        }

        public async Task AddAsync(Event @event)
        {
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();
        }

        public async Task AddEventCategoriesAsync(IEnumerable<EventCategory> eventCategories)
        {
            await _context.EventCategories.AddRangeAsync(eventCategories);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Event @event)
        {
            _context.Events.Update(@event);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Event @event)
        {
            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
        }
    }
}
