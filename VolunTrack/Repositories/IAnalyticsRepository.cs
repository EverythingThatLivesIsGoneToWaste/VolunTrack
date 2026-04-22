using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public interface IAnalyticsRepository
    {
        // Regular methods
        Task<Dictionary<string, decimal>> GetUserHoursStatsAsync(int userId); // Status - Hours
        Task<Category?> GetUserFavoriteCategoryAsync(int userId);
        Task<decimal> GetTotalConfirmedHoursAsync(int userId);

        // Universal methods
        Task<Dictionary<string, List<Event>>> GetUserEventsStatsAsync(int? userId = null); // Category - Events

        // Admin methods
        Task<Dictionary<Event, int>> GetParticipantsCountByEventsAsync(); // Event - Participants
        Task<List<Event>> GetCompletedEventsByMonth(); // Events grouped by months
        Task<Dictionary<string, decimal>> GetTotalHoursByCategoryAsync();
        Task<int> GetTotalActiveUsersAsync();
        Task<Dictionary<string, int>> GetUserRegistrationsByMonthAsync();
    }
}
