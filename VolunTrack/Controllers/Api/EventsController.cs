using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Exceptions;
using VolunTrack.Repositories;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Api
{
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(
            IEventRepository eventRepository,
            IEventService eventService,
            ILogger<EventsController> logger)
        {
            _eventRepository = eventRepository;
            _eventService = eventService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role)!;

            var events = await _eventService.GetEventsAsync("upcoming", userId, userRole);
            return Ok(events);
        }

        [Authorize(Roles = "EventCoordinator,RegionCoordinator,Administrator")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role)!;

            var events = await _eventService.GetEventsAsync("my", userId, userRole);
            return Ok(events);
        }

        [Authorize(Roles = "EventCoordinator,Administrator")]
        [HttpPatch("{eventId}/status")]
        public async Task<IActionResult> ChangeEventStatus(int eventId, [FromBody] EventStatus newStatus)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role)!;

                var result = await _eventService.UpdateStatusAsync(eventId, newStatus, userId, userRole);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, "Event does not exist");
                return NotFound(new { message = ex.Message });
            }
            catch (Exceptions.UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized attempt to change event status");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing event status");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize(Roles = "EventCoordinator,Administrator")]
        [HttpGet("{eventId}/template")]
        public async Task<IActionResult> GetEventTemplate(int eventId)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId);
            if (eventEntity == null)
                return NotFound();

            var template = new CreateEventDto
            {
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                Place = eventEntity.Place,
                SkillsRequired = eventEntity.SkillsRequired,
                CategoryIds = [..eventEntity.EventCategories.Select(ec => ec.CategoryId)]
            };

            return Ok(template);
        }

        [Authorize]
        [HttpGet("{eventId}/participants")]
        public async Task<IActionResult> GetEventParticipants(int eventId) {
            var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claimsUserIdString, out var userId))
                return BadRequest("Invalid user ID in token");

            var eventEntity = await _eventRepository.GetByIdAsync(eventId);
            if (eventEntity == null)
                return NotFound();

            var events = await _eventService.GetEventParticipantsAsync(eventId);
            return Ok(events);
        }

        [Authorize(Roles = "EventCoordinator,Administrator")]
        [HttpPost("{eventId}/leader")]
        public async Task<IActionResult> ToggleEventLeader(int eventId, [FromBody] ToggleLeaderDto dto)
        {
            try
            {
                var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(claimsUserIdString, out int claimsUserId))
                    return BadRequest("Invalid user ID in token");

                if (claimsUserId == dto.UserId)
                {
                    return Conflict(new { message = "Authorized users cannot be assigned as event leaders" });
                }

                var userRole = User.FindFirstValue(ClaimTypes.Role)!;

                var leader = await _eventService.ToggleEventLeaderAsync(eventId, dto.UserId, claimsUserId, userRole);
                return Ok(leader);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, "Some data not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exceptions.UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized attempt to asign event leader");
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (EventLeaderLimitExceededException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning event leader");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
