using VolunTrack.Models;
using VolunTrack.Repositories;
using VolunTrack.DTO;

namespace VolunTrack.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;

        public UserService(
            IUserRepository userRepository,
            ICategoryRepository categoryRepository)
        {
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
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
    }
}
