using LiberaMais.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{

    [PaginaParaUsuarioLogado]
    public class RestritoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
