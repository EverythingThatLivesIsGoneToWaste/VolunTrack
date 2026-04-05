using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Exceptions;
using VolunTrack.Repositories;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Api
{
    [Route("api/events")]
    [ApiController]
    public class EventFilesController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventFilesController(
            IEventRepository eventRepository,
            IEventService eventService,
            ILogger<EventsController> logger)
        {
            _eventRepository = eventRepository;
            _eventService = eventService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("/api/events/{eventId}/photos")]
        public async Task<IActionResult> GetPhotos(int eventId)
        {
            try
            {
                var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(claimsUserIdString, out _))
                    return BadRequest("Invalid user ID in token");

                var photos = await _eventService.GetEventPhotosAsync(eventId);
                return Ok(photos);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photos for event {EventId}", eventId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize(Roles = "Volunteer,EventCoordinator,Administrator")]
        [HttpPost("/api/events/{eventId}/photos")]
        public async Task<IActionResult> UploadPhotos(int eventId, [FromForm] UploadPhotoDto dto)
        {
            try
            {
                var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(claimsUserIdString, out int userId))
                    return BadRequest("Invalid user ID in token");

                var addedPhoto = await _eventService.AddEventPhotoAsync(eventId, dto, userId);
                return Ok(addedPhoto);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exceptions.UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo for event {EventId}", eventId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpDelete("/api/events/photos/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            try
            {
                var claimsUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(claimsUserIdString, out int userId))
                    return BadRequest("Invalid user ID in token");

                await _eventService.RemoveEventPhotoAsync(photoId, userId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exceptions.UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing photo {photoId}", photoId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
