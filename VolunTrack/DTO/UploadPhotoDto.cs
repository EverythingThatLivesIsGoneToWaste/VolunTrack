using System.ComponentModel.DataAnnotations;
using VolunTrack.Enums;

namespace VolunTrack.DTO
{
    public class UploadPhotoDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Required]
        public PhotoType PhotoType { get; set; }

        [StringLength(80)]
        public string? Title { get; set; }
    }
}
