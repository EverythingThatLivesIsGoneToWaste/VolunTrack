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
        private readonly ILoginService _loginService;
        private readonly ILogger<LoginController> _logger;
        private readonly IUserRepository _userRepository;

        public DashboardController(
            ILoginService loginService,
            ILogger<LoginController> logger,
            IUserRepository userRepository)
        {
            _loginService = loginService;
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
            var userCategories = await _userRepository.GetUserCategoriesAsync(userId);

            if (user == null)
            {
                _logger.LogError("User with id {UserId} not found in database", userId);
                await _loginService.LogoutAsync();
                return RedirectToAction("Index", "Login");
            }

            var roleFromDb = user.Role.ToString();
            var roleFromClaim = User.FindFirstValue(ClaimTypes.Role);

            if (roleFromDb != roleFromClaim)
            {
                await _loginService.LogoutAsync();
                TempData["LogoutReason"] = "Ваша роль была изменена. Пожалуйста, войдите заново";
                return RedirectToAction("Index", "Login");
            }

            var viewModel = new DashboardViewModel
            {
                CurrentUser = UserDto.FromEntity(user, userCategories)
            };

            return View(viewModel);
        }
    }
}
