using System.ComponentModel.DataAnnotations;

namespace VolunTrack.DTO
{
    public class RegisterDto
    {
        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public List<int> CategoryIds { get; set; } = [];
    }
}
