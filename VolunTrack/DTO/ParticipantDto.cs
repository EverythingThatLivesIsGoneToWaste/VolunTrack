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

        public bool IsConfirmedByCoordinator { get; set; }
        public bool IsConfirmedByLeader { get; set; }
        public decimal TotalHours { get; set; }

        public static ParticipantDto FromEntity(Participation participation)
        {
            return new ParticipantDto
            {
                UserId = participation.User.Id,
                ParticipationId = participation.Id,
                Login = participation.User.Login,
                FullName = participation.User.FullName,
                Email = participation.User.Email,
                Role = participation.User.Role,

                IsConfirmedByCoordinator = participation.IsConfirmedByCoordinator,
                IsConfirmedByLeader = participation.IsConfirmedByLeader,
                TotalHours = participation.TotalHours
            };
        }
    }
}
