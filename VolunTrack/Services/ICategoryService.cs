using VolunTrack.DTO;

namespace VolunTrack.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetCategoriesAsync(string? userRole);
        Task<CategoryDto> CreateAsync(CreateCategoryDto model);
        Task<CategoryDto> UpdateAsync(UpdateCategoryDto model);
        Task<CategoryDto> ToggleActivityAsync(int categoryId);
        Task DeleteAsync(int categoryId);
    }
}
