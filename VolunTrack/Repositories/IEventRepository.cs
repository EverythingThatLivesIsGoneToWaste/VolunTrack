using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(int id);
        Task<List<Event>> GetAllAsync(string? searchTerm = null);
        Task<List<Event>> GetUpcomingAsync(string? searchTerm = null);
        Task<List<Event>> GetByCategoryIdsAsync(List<int> categoryIds);
        Task<List<Event>> GetByCoordinatorIdAsync(int coordinatorId, string? searchTerm = null);
        Task<List<Event>> GetEventsToUpdateStatusAsync();
        Task<List<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end);
        Task<EventPhoto?> GetPhotoByIdAsync(int photoId);
        Task<Attachment?> GetDocumentByIdAsync(int attachmentId);

        Task AddAsync(Event @event);
        Task AddEventCategoriesAsync(IEnumerable<EventCategory> eventCategories);
        Task UpdateAsync(Event @event);
        Task DeleteAsync(Event @event);

        Task<List<EventPhoto>> GetPhotosByEventIdAsync(int eventId);
        Task AddPhotoAsync(EventPhoto photo);
        Task DeletePhotoAsync(EventPhoto photo);

        Task<List<Attachment>> GetDocumentsByEventIdAsync(int eventId);
        Task<int> GetEventDocumentsCountAsync(int eventId);
        Task AddDocumentAsync(Attachment attachment);
        Task DeleteDocumentAsync(Attachment attachment);

        Task<bool> IsUserLeaderOfEventAsync(int eventId, int userId);
        Task<int> GetEventLeadersCountAsync(int eventId);
        Task<List<int>> GetEventLeadersIdsAsync(int eventId);
        Task AssignLeaderAsync(UserLeaderAssignment assignment);
        Task RemoveLeaderAsync(int userId, int eventId);
    }
}
