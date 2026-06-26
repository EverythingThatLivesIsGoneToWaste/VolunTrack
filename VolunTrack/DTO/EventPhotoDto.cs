using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.DTO
{
    public class EventPhotoDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int? UploadedByUserId { get; set; }
        public string? UploadedByUserName { get; set; }
        public PhotoType PhotoType { get; set; }
        public DateTime UploadedAtUtc { get; set; }

        public static EventPhotoDto FromEntity(EventPhoto photo, User? uploadedBy = null)
        {
            return new EventPhotoDto
            {
                Id = photo.Id,
                Title = photo.Title,
                FilePath = photo.FilePath,
                UploadedByUserId = photo.UploadedByUserId,
                UploadedByUserName = uploadedBy?.FullName ?? "Unknown",
                PhotoType = photo.PhotoType,
                UploadedAtUtc = photo.UploadedAtUtc
            };
        }
    }
}
