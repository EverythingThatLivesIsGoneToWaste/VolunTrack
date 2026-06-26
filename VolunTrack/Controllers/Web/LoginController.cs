using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunTrack.DTO;
using VolunTrack.Exceptions;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Web
{
    public class LoginController : Controller
    {
        private readonly ILoginService _authService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(
            ILoginService authService,
            ILogger<LoginController> logger)
        {
            _authService = authService;
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
        public async Task<IActionResult> Index(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = await _authService.AuthenticateAsync(model.Login, model.Password);
                await _authService.LoginAsync(user);

                _logger.LogInformation("User {Login} logged in", user.Login);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (UnauthorizedException)
            {
                _logger.LogWarning("Failed login attempt for {Login}", model.Login);
                ModelState.AddModelError(string.Empty, "Invalid login or password");
                Response.StatusCode = 401;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Login}", model.Login);
                ModelState.AddModelError(string.Empty, "Internal server error");
                Response.StatusCode = 500;
                return View(model);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
