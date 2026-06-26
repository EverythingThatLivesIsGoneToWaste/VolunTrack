using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.DTO
{
    public class ParticipantDto
    {
        public int UserId { get; set; }
        public int ParticipationId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsLeader { get; set; }
        public ParticipationStatus ParticipationStatus { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public bool IsConfirmedByCoordinator { get; set; }
        public bool IsConfirmedByLeader { get; set; }
        public decimal TotalHours { get; set; }

        public static ParticipantDto FromEntity(Participation participation, bool isLeader)
        {
            return new ParticipantDto
            {
                UserId = participation.User.Id,
                ParticipationId = participation.Id,
                Login = participation.User.Login,
                FullName = participation.User.FullName,
                Email = participation.User.Email,
                Role = participation.User.Role,
                IsLeader = isLeader,
                ParticipationStatus = participation.Status,
                CheckInTime = participation.CheckInTime,
                CheckOutTime = participation.CheckOutTime,

                IsConfirmedByCoordinator = participation.IsConfirmedByCoordinator,
                IsConfirmedByLeader = participation.IsConfirmedByLeader,
                TotalHours = participation.TotalHours
            };
        }
    }
}
