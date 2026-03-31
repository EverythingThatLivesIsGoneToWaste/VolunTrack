using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Exceptions;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Api
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController( 
            ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            try
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role)!;

                var categories = await _categoryService.GetCategoriesAsync(userRole);
                return Ok(categories);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> Update(int categoryId, [FromBody] UpdateCategoryDto dto)
        {
            if (categoryId != dto.Id)
                return BadRequest(new { message = "ID mismatch" });

            try
            {
                var result = await _categoryService.UpdateAsync(dto);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (AlreadyExistsException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPatch("{categoryId}/toggle")]
        public async Task<IActionResult> ToggleActivity(int categoryId)
        {
            try
            {
                await _categoryService.ToggleActivityAsync(categoryId);
                return Ok();
            }
            catch (NotFoundException ex) {
                return NotFound(new { message = ex.Message });
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> Delete(int categoryId)
        {
            try
            {
                await _categoryService.DeleteAsync(categoryId);
                return Ok();
            }
            catch (NotFoundException ex) {
                return NotFound(new { message = ex.Message });
            }
            catch (RelationsExistException ex) {
                return Conflict(new { message = ex.Message });
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
