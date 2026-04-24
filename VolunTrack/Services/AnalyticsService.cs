using VolunTrack.DTO;
using VolunTrack.Repositories;

namespace VolunTrack.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        public AnalyticsService(IAnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }

        public async Task<AdminStatsDto> GetAdminStatsAsync()
        {
            var currentYear = DateTime.UtcNow.Year;

            var eventsStats = await _analyticsRepository.GetUserEventsStatsAsync();
            var eventParticipantsStats = await _analyticsRepository.GetParticipantsCountByEventsAsync();

            return new AdminStatsDto
            {
                EventsByCategory = eventsStats.ToDictionary(k => k.Key, v => v.Value.Select(e => EventDto.FromEntity(e)).ToList()),
                EventParticipantsCount = eventParticipantsStats.ToDictionary(k => EventDto.FromEntity(k.Key), v => v.Value),
                EventsByMonths = await _analyticsRepository.GetCompletedEventsByMonthAsync(currentYear),
                TotalHoursByCategory = await _analyticsRepository.GetTotalHoursByCategoryAsync(),
                TotalUsers = await _analyticsRepository.GetTotalActiveUsersAsync(),
                RegistrationsByMonth = await _analyticsRepository.GetUserRegistrationsByMonthAsync(currentYear),
                TotalCompletedEvents = await _analyticsRepository.GetTotalCompletedEventsAsync()
            };
        }

        public async Task<UserStatsDto> GetUserStatsAsync(int userId)
        {
            var eventsStats = await _analyticsRepository.GetUserEventsStatsAsync(userId);

            return new UserStatsDto
            {
                HoursByStatus = await _analyticsRepository.GetUserHoursStatsAsync(userId),
                FavoriteCategory = CategoryDto.FromEntity(await _analyticsRepository.GetUserFavoriteCategoryAsync(userId)),
                TotalConfirmedHours = await _analyticsRepository.GetTotalConfirmedHoursAsync(userId),
                EventsByCategory = eventsStats.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(e => EventDto.FromEntity(e)).ToList()
                ),
                TotalCompletedEvents = await _analyticsRepository.GetUserTotalCompletedEventsAsync(userId)
            };
        }
    }
}
