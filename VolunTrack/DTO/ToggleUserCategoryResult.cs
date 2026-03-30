using VolunTrack.Enums;

namespace VolunTrack.DTO
{
    public class ToggleUserCategoryResult
    {
        public bool IsSuccess { get; set; }
        public ToggleUserCategoryStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;

        private ToggleUserCategoryResult() { }

        public static ToggleUserCategoryResult Assigned() => new()
        {
            IsSuccess = true,
            Status = ToggleUserCategoryStatus.CategoryAssigned,
            Message = "Категория успешно присвоена"
        };

        public static ToggleUserCategoryResult Removed() => new()
        {
            IsSuccess = true,
            Status = ToggleUserCategoryStatus.CategoryRemoved,
            Message = "Категория успешно удалена"
        };

        public static ToggleUserCategoryResult UserNotFound(int userId) => new()
        {
            IsSuccess = false,
            Status = ToggleUserCategoryStatus.UserNotFound,
            Message = $"Пользователь {userId} не найден"
        };

        public static ToggleUserCategoryResult CategoryNotFound(int categoryId) => new()
        {
            IsSuccess = false,
            Status = ToggleUserCategoryStatus.CategoryNotFound,
            Message = $"Категория {categoryId} не найдена"
        };

        public static ToggleUserCategoryResult CategoryInactive(int categoryId) => new()
        {
            IsSuccess = false,
            Status = ToggleUserCategoryStatus.CategoryInactive,
            Message = $"Категория {categoryId} неактивна и не может быть присвоена"
        };
    }
}
