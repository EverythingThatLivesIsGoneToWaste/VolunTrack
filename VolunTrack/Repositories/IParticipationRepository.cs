using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public interface IParticipationRepository
    {
        Task<Participation?> GetByIdAsync(int id);
        Task<List<Participation>> GetByUserIdAsync(int userId);
        Task<Participation?> GetByUserAndEventAsync(int userId, int eventId);
        Task<List<Event>> GetUpcomingEventsByUserIdAsync(int userId);
        Task<List<Event>> GetCompletedEventsByUserIdAsync(int userId);
        Task<List<Participation>> GetByEventIdAsync(int eventId);

        Task AddAsync(Participation participation);
        Task UpdateAsync(Participation participation);
        Task MarkManualCheckOutAsync(Participation participation, DateTime checkOutTime);
        Task UpdateConfirmationAsync(Participation participation, bool byCoordinator, bool byLeader);

        Task<bool> ExistsAsync(int userId, int eventId);
    }
}
