using VolunTrack.Enums;

namespace VolunTrack.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Place { get; set; } = string.Empty;
        public EventStatus Status { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int EstimatedParticipantsCount { get; set; }
        public string SkillsRequired { get; set; } = string.Empty;
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public List<EventPhoto> EventPhotos { get; set; } = [];
        public List<Participation> Participations { get; set; } = [];
        public List<UserLeaderAssignment> LeaderAssignments { get; set; } = [];
        public List<EventCategory> EventCategories { get; set; } = [];

        public User? CreatedByUser { get; set; }
    }
}
