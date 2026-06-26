using VolunTrack.DTO;

namespace VolunTrack.Services
{
    public interface IRegistrationService
    {
        Task<RegistrationResponse> RegisterAsync(RegisterDto dto);
    }
}
