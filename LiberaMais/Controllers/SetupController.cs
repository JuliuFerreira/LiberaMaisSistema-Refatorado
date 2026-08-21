using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class SetupController : Controller
    {

        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly ISessao _sessao;

        public SetupController(IUsuarioRepositorio usuarioRepositorio, ISessao sessao)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _sessao = sessao;
        }


        public IActionResult Index()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            return View();
        }
    }
}
