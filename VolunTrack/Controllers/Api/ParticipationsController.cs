using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Exceptions;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Api
{
    [Route("api/participations")]
    [ApiController]
    public class ParticipationsController : ControllerBase
    {
        private readonly IParticipationService _participationService;
        private readonly ILogger<ParticipationsController> _logger;

        public ParticipationsController(
            IParticipationService participationService, 
            ILogger<ParticipationsController> logger)
        {
            _participationService = participationService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Join([FromBody] JoinEventDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var result = await _participationService.JoinAsync(userId, dto.EventId);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("User {UserId} successfully joined event {EventId}", userId, dto.EventId);
                    return Ok(result);
                }

                return result.Status switch
                {
                    JoinEventStatus.EventNotFound => NotFound(result),
                    JoinEventStatus.EventCancelled => Conflict(result),
                    JoinEventStatus.EventNotPublished => Conflict(result),
                    JoinEventStatus.EventAlreadyStarted => Conflict(result),
                    JoinEventStatus.AlreadyJoined => Conflict(result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining event {EventId}", dto.EventId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPut("/api/participations/{participationId}/hours")]
        public async Task<IActionResult> UpdateParticipationHours(int participationId, [FromBody] UpdateHoursDto dto)
        {
            try
            {
                var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(claimsUserIdString, out int claimsUserId))
                    return BadRequest("Invalid user ID in token");

                var userRole = User.FindFirstValue(ClaimTypes.Role)!;

                var result = await _participationService.UpdateHoursAsync(participationId, dto, claimsUserId, userRole);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (TimeNotRecordedException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exceptions.UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error recording participation time");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("/api/participations/{participationId}/hours/confirm")]
        public async Task<IActionResult> ConfirmParticipationHours(int participationId, [FromBody] ConfirmHoursDto dto)
        {
            try
            {
                var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(claimsUserIdString, out int claimsUserId))
                    return BadRequest("Invalid user ID in token");

                var userRole = User.FindFirstValue(ClaimTypes.Role)!;

                var result = await _participationService.ConfirmHoursAsync(participationId, dto, claimsUserId, userRole);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (TimeNotRecordedException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exceptions.UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error recording participation time");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("/api/participations/{participationId}/hours/reject")]
        public async Task<IActionResult> RejectHours(int participationId)
        {
            try
            {
                var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(claimsUserIdString, out int claimsUserId))
                    return BadRequest("Invalid user ID in token");

                var userRole = User.FindFirstValue(ClaimTypes.Role)!;

                var result = await _participationService.RejectHoursAsync(participationId, claimsUserId, userRole);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (TimeNotRecordedException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exceptions.UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording participation time");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
