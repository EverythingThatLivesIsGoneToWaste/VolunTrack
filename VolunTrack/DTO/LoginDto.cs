using System.ComponentModel.DataAnnotations;

namespace VolunTrack.DTO
{
    public class LoginDto
    {
        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
