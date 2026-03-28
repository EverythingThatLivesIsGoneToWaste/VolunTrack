using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunTrack.DTO;
using VolunTrack.Exceptions;
using VolunTrack.Services;

namespace VolunTrack.Controllers.Web
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, 
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult Add() => View();

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Add(CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _categoryService.CreateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (AlreadyExistsException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                Response.StatusCode = 409;
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during category {Action}", nameof(Add));
                ModelState.AddModelError(string.Empty, "Internal server error");
                Response.StatusCode = 500;
                return View(dto);
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try { 
                await _categoryService.UpdateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException ex) {
                ModelState.AddModelError(string.Empty, ex.Message);
                Response.StatusCode = 404;
                return View(dto);
            }
            catch (AlreadyExistsException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                Response.StatusCode = 409;
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during category {Action}", nameof(Edit));
                ModelState.AddModelError(string.Empty, "Internal server error");
                Response.StatusCode = 500;
                return View(dto);
            }
        }
    }
}
