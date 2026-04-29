using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public interface IAnalyticsRepository
    {
        // Regular methods
        Task<Dictionary<string, decimal>> GetUserHoursStatsAsync(int userId); // Status - Hours
        Task<Category?> GetUserFavoriteCategoryAsync(int userId);
        Task<decimal> GetTotalConfirmedHoursAsync(int userId);
        Task<int> GetUserTotalCompletedEventsAsync(int userId);

        // Universal methods
        Task<Dictionary<string, List<Event>>> GetUserEventsStatsAsync(int? userId = null); // Category - Events

        // Admin methods
        Task<Dictionary<Event, int>> GetParticipantsCountByEventsAsync(); // Event - Participants
        Task<Dictionary<int, int>> GetCompletedEventsByMonthAsync(int year); // Events grouped by months
        Task<Dictionary<Category, decimal>> GetTotalHoursByCategoryAsync();
        Task<int> GetTotalActiveUsersAsync();
        Task<Dictionary<int, int>> GetUserRegistrationsByMonthAsync(int year);
        Task<int> GetTotalCompletedEventsAsync();
    }
}
