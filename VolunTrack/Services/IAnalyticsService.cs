using VolunTrack.DTO;

namespace VolunTrack.Services
{
    public interface IAnalyticsService
    {
        Task<UserStatsDto> GetUserStatsAsync(int userId);
        Task<AdminStatsDto> GetAdminStatsAsync();
    }
}
