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
                .Include(e => e.EventPhotos)
                .Include(e => e.Participations)
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
                .Include(e => e.EventPhotos)
                .Include(e => e.Participations)
                .Where(e => e.CreatedByUserId == coordinatorId).ToListAsync();
        }

        public async Task<List<Event>> GetEventsToUpdateStatusAsync()
        {
            return await _context.Events
                .Where(e => e.Status != EventStatus.Cancelled &&
                    e.Status != EventStatus.Draft)
                .ToListAsync();
        }

        public async Task<List<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Events
                .Where(e => e.StartDateTime >= start && e.StartDateTime < end)
                .Include(e => e.Participations)
                .ToListAsync();
        }

        public async Task<EventPhoto?> GetPhotoByIdAsync(int photoId)
        {
            return await _context.EventPhotos.FirstOrDefaultAsync(p => p.Id == photoId);
        }

        public async Task<Attachment?> GetDocumentByIdAsync(int documentId)
        {
            return await _context.Attachments.FirstOrDefaultAsync(d => d.Id == documentId);
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

        public async Task<List<EventPhoto>> GetPhotosByEventIdAsync(int eventId)
        {
            return await _context.EventPhotos
                .Where(p => p.EventId == eventId)
                .OrderByDescending(p => p.UploadedAtUtc)
                .ToListAsync();
        }

        public async Task AddPhotoAsync(EventPhoto photo)
        {
            await _context.EventPhotos.AddAsync(photo);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePhotoAsync(EventPhoto photo)
        {
            _context.EventPhotos.Remove(photo);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Attachment>> GetDocumentsByEventIdAsync(int eventId)
        {
            return await _context.Attachments
                .Where(a => a.EntityId == eventId && a.EntityType == EntityType.Event)
                .OrderByDescending(a => a.UploadedAtUtc)
                .ToListAsync();
        }

        public async Task<int> GetEventDocumentsCountAsync(int eventId)
        {
            return await _context.Attachments
                .Where(a => a.EntityId == eventId && a.EntityType == EntityType.Event)
                .CountAsync();
        }

        public async Task AddDocumentAsync(Attachment attachment)
        {
            await _context.Attachments.AddAsync(attachment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDocumentAsync(Attachment attachment)
        {
            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsUserLeaderOfEventAsync(int eventId, int userId)
        {
            return await _context.UserLeaderAssignments
                .AnyAsync(ula => ula.EventId == eventId && ula.UserId == userId);
        }

        public async Task<int> GetEventLeadersCountAsync(int eventId)
        {
            return await _context.UserLeaderAssignments
                .Where(ula => ula.EventId == eventId)
                .CountAsync();
        }

        public async Task<List<int>> GetEventLeadersIdsAsync(int eventId)
        {
            return await _context.UserLeaderAssignments
                .Where(ula => ula.EventId == eventId)
                .Select(ula => ula.UserId)
                .ToListAsync();
        }

        public async Task AssignLeaderAsync(UserLeaderAssignment assignment)
        {
            await _context.UserLeaderAssignments.AddAsync(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveLeaderAsync(int userId, int eventId)
        {
            var assignment = await _context.UserLeaderAssignments
                .FirstOrDefaultAsync(ula => ula.EventId == eventId && ula.UserId == userId);

            if (assignment != null)
            {
                _context.UserLeaderAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
