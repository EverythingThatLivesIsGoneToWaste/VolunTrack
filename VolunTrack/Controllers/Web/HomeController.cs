using Microsoft.AspNetCore.Mvc;

namespace VolunTrack.Controllers.Web
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction("Index", "Login");
        }
    }
}
