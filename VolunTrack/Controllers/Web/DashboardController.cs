using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Models.ViewModels;
using VolunTrack.Repositories;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Web
{
    public class DashboardController : Controller
    {
        private readonly ILoginService _authService;
        private readonly ILogger<LoginController> _logger;
        private readonly IUserRepository _userRepository;

        public DashboardController(
            ILoginService authService,
            ILogger<LoginController> logger,
            IUserRepository userRepository)
        {
            _authService = authService;
            _logger = logger;
            _userRepository = userRepository;
        }

        [Authorize]

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Invalid user id claim");
                return RedirectToAction("Index", "Login");
            }

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError("User with id {UserId} not found in database", userId);
                await _authService.LogoutAsync();
                return RedirectToAction("Index", "Login");
            }

            var viewModel = new DashboardViewModel
            {
                CurrentUser = UserDto.FromEntity(user)
            };

            return View(viewModel);
        }
    }
}
