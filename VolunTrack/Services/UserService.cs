using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Exceptions;
using VolunTrack.Models;
using VolunTrack.Repositories;

namespace VolunTrack.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILoginService _loginService;

        public UserService(
            IUserRepository userRepository,
            ICategoryRepository categoryRepository,
            ILoginService loginService)
        {
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _loginService = loginService;
        }

        public async Task<ToggleUserCategoryResult> ToggleUserCategory(int userId, int categoryId)
        {
            var userEntity = await _userRepository.GetByIdAsync(userId);
            if (userEntity == null)
                return ToggleUserCategoryResult.UserNotFound(userId);

            var categoryEntity = await _categoryRepository.GetByIdAsync(categoryId);
            if (categoryEntity == null)
                return ToggleUserCategoryResult.CategoryNotFound(categoryId);

            if (!categoryEntity.IsActive)
                return ToggleUserCategoryResult.CategoryInactive(categoryId);

            var userCategoryEntity = await _userRepository.GetUserCategoryAsync(userId, categoryId);

            if (userCategoryEntity == null) {
                var userCategory = new UserCategory
                {
                    UserId = userId,
                    CategoryId = categoryId
                };

                await _userRepository.AddUserCategoryAsync(userCategory);
                return ToggleUserCategoryResult.Assigned();
            } else
            {
                await _userRepository.RemoveUserCategoryAsync(userCategoryEntity);
                return ToggleUserCategoryResult.Removed();
            }
        }

        public async Task<List<CategoryDto>> GetUserCategoriesAsync(int userId)
        {
            var categories = await _userRepository.GetUserCategoriesAsync(userId);
            return [..categories.Select(c => CategoryDto.FromEntity(c))];
        }

        public async Task<UserDto> SetUserRoleAsync(int userId, UserRole userRole)
        {
            var userEntity = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException($"User {userId} not found");

            userEntity.Role = userRole;
            await _userRepository.UpdateAsync(userEntity);

            return UserDto.FromEntity(userEntity);
        }
    }
}
