using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Exceptions;
using VolunTrack.Models;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Api
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService, 
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [Authorize]
        [HttpPatch("{userId}/categories/{categoryId}/toggle")]
        public async Task<IActionResult> ToggleUserCategory(int userId, int categoryId)
        {
            var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claimsUserIdString, out var claimsUserId))
                return BadRequest("Invalid user ID in token");

            if (claimsUserId != userId)
                return Forbid();

            try
            {
                var result = await _userService.ToggleUserCategory(userId, categoryId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("User {userId} successfully assigned/removed category {categoryId} from their profile", 
                        userId, categoryId);
                    return Ok(result);
                }

                return result.Status switch
                {
                    ToggleUserCategoryStatus.UserNotFound => NotFound(result),
                    ToggleUserCategoryStatus.CategoryNotFound => NotFound(result),
                    ToggleUserCategoryStatus.CategoryInactive => Conflict(result),
                    _ => BadRequest(result)
                };
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category {categoryId} for user {userId}", userId, categoryId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("me/categories")]
        public async Task<IActionResult> GetMyCategories()
        {
            var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claimsUserIdString, out var userId))
                return BadRequest("Invalid user ID in token");

            var categories = await _userService.GetUserCategoriesAsync(userId);
            return Ok(categories);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPatch("/api/users/{userId}/role")]
        public async Task<IActionResult> SetUserRole(int userId, [FromBody] SetUserRoleDto dto)
        {
            var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claimsUserIdString, out var claimsUserId))
                return BadRequest("Invalid user ID in token");

            if (claimsUserId == userId)
            {
                return Conflict(new {message = "Administrator can't update own role" });
            }

            try
            {
                var result = await _userService.SetUserRoleAsync(userId, dto.UserRole);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing role to {userRole} for user {userId}", dto.UserRole, userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string? search)
        {
            var users = await _userService.GetUsersAsync(search);
            return Ok(users);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPatch("{userId}/activity/toggle")]
        public async Task<IActionResult> ToggleUserActivity(int userId)
        {
            var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claimsUserIdString, out var claimsUserId))
                return BadRequest("Invalid user ID in token");

            if (claimsUserId == userId)
            {
                return Conflict(new { message = "Administrator can't update own activity status" });
            }

            try
            {
                var result = await _userService.ToggleUserActivityAsync(userId);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling activity status for user {userId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("me/events/upcoming")]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claimsUserIdString, out var userId))
                return BadRequest("Invalid user ID in token");

            var events = await _userService.GetUpcomingEventsAsync(userId);
            return Ok(events);
        }

        [Authorize]
        [HttpGet("me/events/completed")]
        public async Task<IActionResult> GetCompletedEvents()
        {
            var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claimsUserIdString, out var userId))
                return BadRequest("Invalid user ID in token");

            var events = await _userService.GetCompletedEventsAsync(userId);
            return Ok(events);
        }
    }
}
