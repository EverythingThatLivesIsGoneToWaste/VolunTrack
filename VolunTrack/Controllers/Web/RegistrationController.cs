using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunTrack.DTO;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Web
{
    public class RegistrationController : Controller
    {
        private readonly IRegistrationService _registrationService;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(
            IRegistrationService registrationService,
            ILogger<RegistrationController> logger)
        {
            _registrationService = registrationService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await _registrationService.RegisterAsync(model);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("User {Login} registered", response.User!.Login);
                    return RedirectToAction("Index", "Login");
                }

                ModelState.AddModelError(string.Empty, response.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Login}", model.Login);
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return View(model);
            }
        }
    }
}
