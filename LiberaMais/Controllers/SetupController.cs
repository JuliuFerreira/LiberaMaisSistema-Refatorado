using LiberaMais.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class SetupController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
