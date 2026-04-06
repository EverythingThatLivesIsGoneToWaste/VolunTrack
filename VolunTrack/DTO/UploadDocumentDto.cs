using System.ComponentModel.DataAnnotations;

namespace VolunTrack.DTO
{
    public class UploadDocumentDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [StringLength(150)]
        public string FileName { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
    }
}
