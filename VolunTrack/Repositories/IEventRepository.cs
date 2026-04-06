using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(int id);
        Task<List<Event>> GetAllAsync();
        Task<List<Event>> GetUpcomingAsync();
        Task<List<Event>> GetByCategoryIdsAsync(List<int> categoryIds);
        Task<List<Event>> GetByCoordinatorIdAsync(int coordinatorId);
        Task<List<Event>> GetEventsToUpdateStatusAsync();
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
        Task AddDocumentAsync(Attachment attachment);
        Task DeleteDocumentAsync(Attachment attachment);
    }
}
