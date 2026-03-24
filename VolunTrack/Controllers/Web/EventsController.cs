using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Web
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(
            IEventService eventService,
            ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [Authorize(Roles = "EventCoordinator,RegionCoordinator,Administrator")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "EventCoordinator,Administrator")]
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateEventDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            model.CreatedByUserId = userId;

            await _eventService.AddAsync(model);

            _logger.LogInformation("User {Id} successfully created event {Name}", model.CreatedByUserId, model.Name);
            return RedirectToAction("Index", "Events");
        }
    }
}
