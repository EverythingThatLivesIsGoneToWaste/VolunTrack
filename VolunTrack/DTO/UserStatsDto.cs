namespace VolunTrack.DTO
{
    public class UserStatsDto
    {
        public Dictionary<string, decimal> HoursByStatus { get; set; } = [];
        public CategoryDto FavoriteCategory { get; set; } = null!;
        public decimal TotalConfirmedHours { get; set; }
        public Dictionary<string, List<EventDto>> EventsByCategory { get; set; } = [];
        public int TotalCompletedEvents { get; set; }
    }
}
