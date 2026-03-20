using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Services;

namespace VolunTrack.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventController> _logger;

        public EventController(
            IEventService eventService,
            ILogger<EventController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [Authorize(Roles = "EventCoordinator")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "EventCoordinator")]
        [HttpPost]
        public async Task<IActionResult> Add(CreateEventDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            model.CreatedByUserId = userId;

            await _eventService.AddAsync(model);

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
