using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Api
{
    [Route("api/analytics")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("stats/user")]
        public async Task<IActionResult> GetUserStatistics()
        {
            var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claimsUserIdString, out int userId))
                return BadRequest("Invalid user ID in token");

            try
            {
                var statisticsDto = await _analyticsService.GetUserStatsAsync(userId);
                return Ok(statisticsDto);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error getting statistics for user {userId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("stats/admin")]
        public async Task<IActionResult> GetAdminStatistics()
        {
            try
            {
                var statisticsDto = await _analyticsService.GetAdminStatsAsync();
                return Ok(statisticsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for admin");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
