using VolunTrack.Enums;

namespace VolunTrack.DTO
{
    public class HoursReportRequestDto
    {
        public int? EventId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public ParticipationStatus? Status { get; set; }
        public bool? Moderated { get; set; }
    }
}
