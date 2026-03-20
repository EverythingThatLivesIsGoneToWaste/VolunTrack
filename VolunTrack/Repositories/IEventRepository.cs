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

        Task AddAsync(Event @event);
        Task AddEventCategoriesAsync(IEnumerable<EventCategory> eventCategories);
        Task UpdateAsync(Event @event);
        Task DeleteAsync(Event @event);
    }
}
