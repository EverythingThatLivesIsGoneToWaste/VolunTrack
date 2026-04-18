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
        private readonly IParticipationRepository _participationRepository;

        public UserService(
            IUserRepository userRepository,
            ICategoryRepository categoryRepository,
            IParticipationRepository participationRepository)
        {
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _participationRepository = participationRepository;
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

        public async Task<List<UserDto>> GetUsersAsync(string? search)
        {
            var users = string.IsNullOrWhiteSpace(search)
                ? await _userRepository.GetAllAsync()
                : await _userRepository.SearchAsync(search);

            return [..users.Select(u => UserDto.FromEntity(u))];
        }

        public async Task<UserDto> ToggleUserActivityAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException($"User {userId} not found");

            user.IsActive = !user.IsActive;
            await _userRepository.UpdateAsync(user);

            return UserDto.FromEntity(user);
        }

        public async Task<List<EventDto>> GetUpcomingEventsAsync(int userId)
        {
            var events = await _participationRepository.GetUpcomingEventsByUserIdAsync(userId);
            return events.Select(e => EventDto.FromEntity(e)).ToList();
        }

        public async Task<List<EventDto>> GetCompletedEventsAsync(int userId)
        {
            var events = await _participationRepository.GetCompletedEventsByUserIdAsync(userId);

            var eventDtos = new List<EventDto>();
            foreach (var e in events)
            {
                var dto = EventDto.FromEntity(e);
                var participation = await _participationRepository.GetByUserAndEventAsync(userId, e.Id);
                if (participation != null)
                {
                    dto.IsHoursRecorded = participation.CheckInTime != null;
                    dto.TotalHours = participation.TotalHours;
                    dto.IsConfirmedByCoordinator = participation.IsConfirmedByCoordinator;
                    dto.IsConfirmedByLeader = participation.IsConfirmedByLeader;
                }

                eventDtos.Add(dto);
            }

            return eventDtos;
        }
    }
}
