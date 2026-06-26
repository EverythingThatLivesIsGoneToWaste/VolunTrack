using VolunTrack.Models;

namespace VolunTrack.DTO
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime UploadedAtUtc { get; set; }
        public int? UploadedByUserId { get; set; }
        public string? UploadedByUserName { get; set; }

        public static DocumentDto FromEntity(Attachment attachment, User? uploadedBy = null)
        {
            return new DocumentDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FilePath = attachment.FilePath,
                Description = attachment.Description,
                UploadedByUserId = attachment.UploadedByUserId,
                UploadedByUserName = uploadedBy?.FullName ?? "Unknown",
                UploadedAtUtc = attachment.UploadedAtUtc
            };
        }
    }
}
