using VolunTrack.Enums;

namespace VolunTrack.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public EntityType EntityType { get; set; }
        public int EntityId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int? UploadedByUserId { get; set; }
        public DateTime UploadedAtUtc { get; set; }

        public User? UploadedByUser { get; set; }
    }
}
