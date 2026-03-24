using Microsoft.AspNetCore.Mvc;
using VolunTrack.DTO;
using VolunTrack.Repositories;

namespace VolunTrack.Controllers.Api
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            return Ok(categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ColorRgb = c.ColorRgb
            }));
        }
    }
}
