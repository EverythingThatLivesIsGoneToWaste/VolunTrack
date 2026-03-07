using VolunTrack.Enums;

namespace VolunTrack.Models
{
    public class EventPhoto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int? UploadedByUserId { get; set; }
        public int EventId { get; set; }
        public PhotoType PhotoType { get; set; }
        public DateTime UploadedAtUtc { get; set; }

        public User? UploadedByUser { get; set; }
        public Event Event { get; set; } = null!;
    }
}
