using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Api
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<EventsController> _logger;

        public ReportsController (
            IReportService reportService, 
            ILogger<EventsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [Authorize(Roles = "Administrator,RegionCoordinator")]
        [HttpGet("/api/reports/users/export")]
        public async Task<IActionResult> ExportUsersReport([FromQuery] bool? isActive, DateOnly? fromDate, DateOnly? toDate)
        {
            try
            {
                var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(claimsUserIdString, out _))
                    return BadRequest("Invalid user ID in token");

                var excelBytes = await _reportService.GenerateUserReportAsync(isActive, fromDate, toDate);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users_report.xlsx");
            }
            catch (Exception ex)
            {
                var status = isActive == null ? "all" : (isActive == true ? "active" : "blocked");

                _logger.LogError(ex, "Error getting {Status} users registered from {FromDate} to {ToDate}", status, fromDate, toDate);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
