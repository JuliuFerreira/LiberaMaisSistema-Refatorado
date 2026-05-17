using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    public class SetupController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
