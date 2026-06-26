namespace VolunTrack.DTO
{
    public class AdminStatsDto
    {
        public Dictionary<string, List<EventDto>> EventsByCategory { get; set; } = [];
        public List<EventParticipantStatDto> EventParticipantsStats { get; set; } = [];
        public Dictionary<int, int> EventsByMonths { get; set; } = [];
 
        public List<TotalCategoryHoursStatDto> TotalHoursByCategory { get; set; } = [];
        public int TotalUsers { get; set; }
        public Dictionary<int, int> RegistrationsByMonth { get; set; } = [];
        public int TotalCompletedEvents { get; set; }
    }
}
