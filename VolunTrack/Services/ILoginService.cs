using VolunTrack.Models;

namespace VolunTrack.Services
{
    public interface ILoginService
    {
        Task<User?> AuthenticateAsync(string login, string password);
        Task LoginAsync(User user);
        Task LogoutAsync();
    }
}
