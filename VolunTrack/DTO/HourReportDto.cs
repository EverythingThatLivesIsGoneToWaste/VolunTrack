using VolunTrack.Enums;

namespace VolunTrack.DTO
{
    public class HourReportDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal RecordedHours { get; set; }
        public ParticipationStatus ParticipationStatus { get; set; }
        public bool ConfirmedByLeader { get; set; }
        public bool ConfirmedByCoordinator { get; set; }
        public bool Moderated { get; set; }
    }
}
