using VolunTrack.Enums;

namespace VolunTrack.Models
{
    public class Participation
    {
        public int Id { get; set; }
        public string Notes { get; set; } = string.Empty;
        public ParticipationStatus Status { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal TotalHours { get; set; }
        public bool IsManualCheckOut { get; set; }
        public bool IsConfirmedByCoordinator { get; set; }
        public bool IsConfirmedByLeader { get; set; }
        public bool HoursModerated { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public Event Event { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
