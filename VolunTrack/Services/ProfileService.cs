using VolunTrack.DTO;
using VolunTrack.Repositories;

namespace VolunTrack.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        public ProfileService(
            IUserRepository userRepository, 
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<ProfileUpdateResult> UpdateProfileAsync(int userId, UserEditDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null) 
                return ProfileUpdateResult.UserNotFound();

            if (user.FullName != dto.FullName)
                user.FullName = dto.FullName;

            if (user.Phone != dto.Phone)
                user.Phone = dto.Phone;

            if (user.Email != dto.Email)
            {
                var emailExists = await _userRepository.ExistsByEmailAsync(dto.Email);
                if (emailExists)
                    return ProfileUpdateResult.EmailAlreadyExists();

                user.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                if (string.IsNullOrEmpty(dto.CurrentPassword))
                    return ProfileUpdateResult.CurrentPasswordRequired();

                if (!_passwordHasher.VerifyPassword(user.PasswordHash, dto.CurrentPassword))
                    return ProfileUpdateResult.InvalidCurrentPassword();

                user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
            }

            await _userRepository.UpdateAsync(user);
            return ProfileUpdateResult.Success();
        }
    }
}
