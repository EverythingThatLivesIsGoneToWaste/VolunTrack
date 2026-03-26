using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Enums;
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
    }
}
