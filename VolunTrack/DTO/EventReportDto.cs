using VolunTrack.Enums;

namespace VolunTrack.DTO
{
    public class EventReportDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Place { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public EventStatus Status { get; set; }
        public int ParticipantsCount { get; set; }
        public decimal TotalHours { get; set; }
        public decimal AverageParticipantsHours { get; set; }
        public string Categories { get; set; } = string.Empty;
    }
}
