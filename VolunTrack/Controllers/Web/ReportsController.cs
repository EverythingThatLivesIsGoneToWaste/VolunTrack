using Microsoft.AspNetCore.Mvc;

namespace VolunTrack.Controllers.Web
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
