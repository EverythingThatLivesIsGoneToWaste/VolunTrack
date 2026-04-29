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
            var totalCategoryHoursStats = await _analyticsRepository.GetTotalHoursByCategoryAsync();

            return new AdminStatsDto
            {
                EventsByCategory = eventsStats.ToDictionary(k => k.Key, v => v.Value.Select(e => EventDto.FromEntity(e)).ToList()),
                EventParticipantsStats = [.. eventParticipantsStats.Select(kvp => new EventParticipantStatDto
                {
                    EventId = kvp.Key.Id,
                    EventName = kvp.Key.Name,
                    ParticipantsCount = kvp.Value
                })],
                EventsByMonths = await _analyticsRepository.GetCompletedEventsByMonthAsync(currentYear),
                TotalHoursByCategory = [.. totalCategoryHoursStats.Select(kvp => new TotalCategoryHoursStatDto {
                    Name = kvp.Key.Name,
                    ColorRgb = kvp.Key.ColorRgb,
                    TotalHours = kvp.Value
                })],
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
