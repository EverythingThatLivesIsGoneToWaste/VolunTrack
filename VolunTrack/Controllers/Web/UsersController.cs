using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VolunTrack.Controllers.Web
{
    public class UsersController : Controller
    {
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult Index() => View();
    }
}
