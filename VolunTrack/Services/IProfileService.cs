using VolunTrack.DTO;

namespace VolunTrack.Services
{
    public interface IProfileService
    {
        Task<ProfileUpdateResult> UpdateProfileAsync(int userId, UserEditDto dto);
    }
}
