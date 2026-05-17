using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    [PaginaParaUsuarioLogado]
    public class PromotoraBancoController : Controller
    {
        private readonly IPromotoraBancoRepositorio _promotoraBancoRepositorio;
        private readonly IPromotorasRepositorio _promotorasRepositorio;
        private readonly IBancosRepositorio _bancosRepositorio;
        private readonly ISessao _sessao;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        public PromotoraBancoController(IPromotoraBancoRepositorio promotoraBancoRepositorio,
                 IPromotorasRepositorio promotorasRepositorio,
                 IBancosRepositorio bancosRepositorio, ISessao sessao, IUsuarioRepositorio usuarioRepositorio)
        {
            _promotoraBancoRepositorio = promotoraBancoRepositorio;
            _promotorasRepositorio = promotorasRepositorio;
            _bancosRepositorio = bancosRepositorio;
            _sessao = sessao;
            _usuarioRepositorio = usuarioRepositorio;
        }


           public IActionResult ListarTodosLogins()
         {
           var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
           ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
        
            List<PromotoraBanco> promotoraBancos = _promotoraBancoRepositorio.ListarPromotoraBanco();
            return View(promotoraBancos);

        }

        public IActionResult Index(int promotoraId)
        {
            var logins = _promotoraBancoRepositorio.ListarPorPromotora(promotoraId);
            var promotora = _promotorasRepositorio.BuscarPromotoraPorId(promotoraId);

            ViewBag.PromotoraNome = promotora.Nome;
            ViewBag.PromotoraId = promotoraId;

            return View(logins);
        }

        public IActionResult Criar(int promotoraId)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            ViewBag.Banco = _bancosRepositorio.ListarBancos();

            var promotora = _promotorasRepositorio.BuscarPromotoraPorId(promotoraId);
            ViewBag.PromotoraId = promotoraId;
            ViewBag.NomePromotora = promotora.Nome;

            return View();

        }

        [HttpPost]
        public IActionResult Criar(PromotoraBanco promotoraBanco)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Não foi possivel salvar o login.";
                ViewBag.Promotora = _promotorasRepositorio.ListarPromotora();
                ViewBag.Banco = _bancosRepositorio.ListarBancos();
                return View(promotoraBanco);
            }

            _promotoraBancoRepositorio.Adicionar(promotoraBanco);
            TempData["MensagemSucesso"] = "Login adicionado com sucesso";
            return RedirectToAction("Index", new { promotoraId = promotoraBanco.PromotoraId });

        }

        public IActionResult Editar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            ViewBag.Banco = _bancosRepositorio.ListarBancos();

            var promotoraBanco = _promotoraBancoRepositorio.BuscarPorId(id);

            if (promotoraBanco == null)
            {
                TempData["MensagemErro"] = "Login não localizado";
                return RedirectToAction("Index");
            }

            ViewBag.NomePromotora = promotoraBanco.Promotora.Nome;
            ViewBag.PromotoraId = promotoraBanco.PromotoraId;

            return View(promotoraBanco);
        }

        [HttpPost]
        public IActionResult Editar(PromotoraBanco promotoraBanco)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;


            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Não foi possivel alterar o login";
                ViewBag.Banco = _bancosRepositorio.ListarBancos();
                ViewBag.NomePromotora = _promotorasRepositorio.BuscarPromotoraPorId(promotoraBanco.PromotoraId).Nome;
                return View(promotoraBanco);
            }

            _promotoraBancoRepositorio.Atualizar(promotoraBanco);
            TempData["MensagemSucesso"] = "Login atualizado com sucesso!";
            return RedirectToAction("Index",new { promotoraId = promotoraBanco.PromotoraId });

        }

        public IActionResult Excluir(int id)
        {
            var promotoraBanco = _promotoraBancoRepositorio.BuscarPorId(id);

            if (promotoraBanco == null)
            {
                TempData["MensagemErro"] = "Login não localizado.";
                return RedirectToAction("Index");
            }

            return View(promotoraBanco);
        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            try
            {
                var promotoraBanco = _promotoraBancoRepositorio.BuscarPorId(id);

                if (promotoraBanco == null)
                {
                    TempData["MensagemErro"] = "Login não localizado.";

                    return RedirectToAction("Index");
                }

                bool apagado = _promotoraBancoRepositorio.Apagar(id);

                if (apagado)
                {
                    TempData["MensagemSucesso"] = "Login excluído com sucesso!";

                    return RedirectToAction(
                        "Index",
                        new { promotoraId = promotoraBanco.PromotoraId });
                }

                TempData["MensagemErro"] = "Não foi possível excluir o login.";

                return RedirectToAction(
                    "Index",
                    new { promotoraId = promotoraBanco.PromotoraId });
            }
            catch (Exception)
            {
                TempData["MensagemErro"] = "Erro ao excluir login.";

                return RedirectToAction("Index");
            }
        }


    }


}
