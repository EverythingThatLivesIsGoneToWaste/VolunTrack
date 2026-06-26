using VolunTrack.Enums;

namespace VolunTrack.DTO
{
    public class UserReportDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegisteredAt { get; set; }
        public decimal TotalConfirmedHours { get; set; }
        public int CompletedEventsCount { get; set; }
    }
}
