using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Exceptions;
using VolunTrack.Models;
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
    }
}
