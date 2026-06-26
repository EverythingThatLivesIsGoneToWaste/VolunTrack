using VolunTrack.DTO;

namespace VolunTrack.Models.ViewModels
{
    public class ProfileEditViewModel
    {
        public UserEditDto EditDto { get; set; } = new();
        public UserDto CurrentUser { get; set; } = null!;
    }
}
