namespace VolunTrack.DTO
{
    public class AdminStatsDto
    {
        public Dictionary<string, List<EventDto>> EventsByCategory { get; set; } = [];
        public Dictionary<EventDto, int> EventParticipantsCount { get; set; } = [];
        public Dictionary<int, int> EventsByMonths { get; set; } = [];
        public Dictionary<string, decimal> TotalHoursByCategory { get; set; } = [];
        public int TotalUsers { get; set; }
        public Dictionary<int, int> RegistrationsByMonth { get; set; } = [];
        public int TotalCompletedEvents { get; set; }
    }
}
