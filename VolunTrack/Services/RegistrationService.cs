using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Models;
using VolunTrack.Repositories;

namespace VolunTrack.Services
{
    public class RegistrationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher) : IRegistrationService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;

        public async Task<RegistrationResponse> RegisterAsync(RegisterDto dto)
        {
            if (string.IsNullOrEmpty(dto.Login) || dto.Login.Length < 3)
                return RegistrationResponse.InvalidLogin();

            if (await _userRepository.ExistsByLoginAsync(dto.Login))
                return RegistrationResponse.LoginTaken();

            if (string.IsNullOrEmpty(dto.Password) || dto.Password.Length < 6)
                return RegistrationResponse.InvalidPassword();

            var emailExists = await _userRepository.ExistsByEmailAsync(dto.Email);
            if (emailExists)
                return RegistrationResponse.EmailAlreadyExists();
            
            var user = new User
            {
                Login = dto.Login,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Email = dto.Email,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = UserRole.Volunteer,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            if (dto.CategoryIds.Count != 0)
            {
                var userCategories = dto.CategoryIds.Select(categoryId => new UserCategory
                {
                    UserId = user.Id,
                    CategoryId = categoryId
                }).ToList();

                await _userRepository.AddUserCategoriesAsync(userCategories);
            }

            var userComplete = await _userRepository.GetByIdAsync(user.Id);

            return RegistrationResponse.Success(UserDto.FromEntity(userComplete!));
        }
    }
}
