using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.DTO
{
    public class ParticipationDto
    {
        public int Id { get; set; }
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

        public static ParticipationDto FromEntity(Participation participation) {
            return new ParticipationDto
            {
                Id = participation.Id,
                Status = participation.Status,
                CheckInTime = participation.CheckInTime,
                CheckOutTime = participation.CheckOutTime,
                TotalHours = participation.TotalHours,
                IsManualCheckOut = participation.IsManualCheckOut,
                IsConfirmedByCoordinator = participation.IsConfirmedByCoordinator,
                IsConfirmedByLeader = participation.IsConfirmedByLeader,
                HoursModerated = participation.HoursModerated,
                EventId = participation.EventId,
                UserId = participation.UserId,
            };
        }
    }
}
