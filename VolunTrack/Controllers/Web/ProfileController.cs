using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Repositories;
using VolunTrack.Services;
using VolunTrack.Models.ViewModels;

namespace VolunTrack.Controllers.Web
{
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly ILogger<LoginController> _logger;
        private readonly IUserRepository _userRepository;

        public ProfileController(
            IProfileService profileService,
            ILogger<LoginController> logger,
            IUserRepository userRepository) 
        {
            _profileService = profileService;
            _logger = logger;
            _userRepository = userRepository;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId);

            var viewModel = new ProfileEditViewModel
            {
                EditDto = new UserEditDto(),
                CurrentUser = UserDto.FromEntity(user)
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(UserEditDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user ID");
            }

            dto ??= new UserEditDto();

            if (!ModelState.IsValid)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                var viewModel = new ProfileEditViewModel
                {
                    EditDto = dto,
                    CurrentUser = UserDto.FromEntity(user)
                };

                return View(viewModel);
            }

            var result = await _profileService.UpdateProfileAsync(userId, dto);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError(string.Empty, result.Message);

            var userForError = await _userRepository.GetByIdAsync(userId);
            var errorViewModel = new ProfileEditViewModel
            {
                EditDto = dto,
                CurrentUser = UserDto.FromEntity(userForError)
            };

            return View(errorViewModel);
        }
    }
}
