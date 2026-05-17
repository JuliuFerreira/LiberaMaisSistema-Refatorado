using LiberaMais.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    [PaginaParaUsuarioLogado]
    public class AppsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
