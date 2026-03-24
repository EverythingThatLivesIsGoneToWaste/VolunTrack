using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.Models;
using VolunTrack.Enums;
using VolunTrack.Repositories;
using VolunTrack.DTO;

namespace VolunTrack.Controllers.Api
{
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;

        public EventsController(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        [Authorize]
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming()
        {
            var events = await _eventRepository.GetUpcomingAsync();
            return Ok(events.Select(e => EventDto.FromEntity(e)));
        }

        [Authorize(Roles = "EventCoordinator,RegionCoordinator,Administrator")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

            List<Event> events;

            if (userRole == UserRole.EventCoordinator)
            {
                events = await _eventRepository.GetByCoordinatorIdAsync(userId);
            }
            else
            {
                events = await _eventRepository.GetAllAsync();
            }

            return Ok(events.Select(e => EventDto.FromEntity(e)));
        }
    }
}
