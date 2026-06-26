using VolunTrack.Models;
using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Repositories;
using VolunTrack.Exceptions;

namespace VolunTrack.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync(string? userRole)
        {
            List<Category> categories;

            if (userRole == nameof(UserRole.Administrator))
            {
                categories = await _categoryRepository.GetAllAsync();
            }
            else
            {
                categories = await _categoryRepository.GetAllActiveAsync();
            }

            return [.. categories.Select(c => CategoryDto.FromEntity(c))];
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto model)
        {
            if (await _categoryRepository.ExistsByNameAsync(model.Name))
                throw new AlreadyExistsException($"Category {model.Name} already exists");
            
            var categoryEntity = new Category
            {
                Name = model.Name,
                Description = model.Description,
                ColorRgb = model.ColorRgb,
                IsActive = true
            };

            await _categoryRepository.AddAsync(categoryEntity);

            return CategoryDto.FromEntity(categoryEntity);
        }

        public async Task<CategoryDto> UpdateAsync(UpdateCategoryDto model)
        {
            var categoryEntity = await _categoryRepository.GetByIdAsync(model.Id)
                ?? throw new NotFoundException($"Category {model.Id} not found");

            if (categoryEntity.Name != model.Name &&
                await _categoryRepository.ExistsByNameAsync(model.Name))
                    throw new AlreadyExistsException($"Category '{model.Name}' already exists");

            categoryEntity.Name = model.Name;
            categoryEntity.Description = model.Description;
            categoryEntity.ColorRgb = model.ColorRgb;
            await _categoryRepository.UpdateAsync(categoryEntity);

            return CategoryDto.FromEntity(categoryEntity);
        }

        public async Task<CategoryDto> ToggleActivityAsync(int categoryId)
        {
            var categoryEntity = await _categoryRepository.GetByIdAsync(categoryId)
                ?? throw new NotFoundException($"Category {categoryId} not found");

            categoryEntity.IsActive = !categoryEntity.IsActive;
            await _categoryRepository.UpdateAsync(categoryEntity);

            return CategoryDto.FromEntity(categoryEntity);
        }

        public async Task DeleteAsync(int categoryId)
        {
            var categoryEntity = await _categoryRepository.GetByIdAsync(categoryId)
                ?? throw new NotFoundException($"Category {categoryId} not found");

            if (await _categoryRepository.RelationsExistAsync(categoryId))
                throw new RelationsExistException($"Category {categoryId} has some relations with Users or Events");

            await _categoryRepository.DeleteAsync(categoryEntity);
        }
    }
}
