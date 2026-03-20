using System.ComponentModel.DataAnnotations;

namespace VolunTrack.DTO
{
    public class CreateEventDto
    {
        [Required]
        [StringLength(80, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Place { get; set; } = string.Empty;

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [StringLength(150)]
        public string? SkillsRequired { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> CategoryIds { get; set; } = [];

        public int CreatedByUserId { get; set; }
    }
}
