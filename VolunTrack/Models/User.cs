using VolunTrack.Enums;

namespace VolunTrack.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
	    public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; }

        public List<Event> CreatedEvents { get; set; } = [];
        public List<Participation> Participations { get; set; } = [];
        public List<UserLeaderAssignment> LeaderAssignments { get; set; } = [];
        public List<Attachment> UploadedAttachments { get; set; } = [];
        public List<UserCategory> Categories { get; set; } = [];
        public List<EventPhoto> UploadedEventPhotos { get; set; } = [];
    }
}
