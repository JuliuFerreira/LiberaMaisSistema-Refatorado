using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using LiberaMais.Services;
using Microsoft.AspNetCore.Authorization;
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
        private readonly PermissaoService _permissaoService;
        public PromotoraBancoController(IPromotoraBancoRepositorio promotoraBancoRepositorio,
                 IPromotorasRepositorio promotorasRepositorio,
                 IBancosRepositorio bancosRepositorio,
                 ISessao sessao,
                 IUsuarioRepositorio usuarioRepositorio,
                 PermissaoService permissaoService)
        {
            _promotoraBancoRepositorio = promotoraBancoRepositorio;
            _promotorasRepositorio = promotorasRepositorio;
            _bancosRepositorio = bancosRepositorio;
            _sessao = sessao;
            _usuarioRepositorio = usuarioRepositorio;
            _permissaoService = permissaoService;
        }


        public IActionResult ListarTodosLogins()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            List<PromotoraBanco> promotoraBancos = _promotoraBancoRepositorio.ListarPromotoraBanco();
            return View(promotoraBancos);

        }

        //[Authorize(Roles ="Admin")]
        public IActionResult Index(int promotoraId)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario(); // Busca a sessão do usuário

            var promotora = _promotorasRepositorio.BuscarPromotoraPorId(promotoraId); // Busca o Id da promotora

            if (!_permissaoService.UsuarioTemAcessoPromotora(usuarioLogado, promotora)) // Serviço que válida se o usuário tem acesso ou não a promotora, depenmdendo do seu perfil,
            {
                TempData["MensagemErro"] =
                    "Você não possui acesso.";

                return RedirectToAction("Index", "Promotora");
            }
            var logins = _promotoraBancoRepositorio.ListarPorPromotora(promotoraId); // Busca os Logins por promotora
            ViewBag.PromotoraId = promotoraId;
            ViewBag.NomePromotora = promotora?.Nome;

            return View(logins);
        }

        [PaginaRestritaSomenteAdmin]
        public IActionResult Criar(int promotoraId)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            var promotora = _promotorasRepositorio.BuscarPromotoraPorId(promotoraId);

            if(!_permissaoService.UsuarioTemAcessoPromotora(usuarioLogado, promotora))
            {
                TempData["MensagemErro"] =
                   "Você não possui acesso.";

                return RedirectToAction("Index", "PromotoraBanco");
            }

            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            ViewBag.Banco = _bancosRepositorio.ListarBancos();
            ViewBag.Usuario = _usuarioRepositorio.ListarTodosUsuarios();

            ViewBag.PromotoraId = promotoraId;
            ViewBag.NomePromotora = promotora.Nome;


            return View();

        }

        [PaginaRestritaSomenteAdmin]
        [HttpPost]
        public IActionResult Criar(PromotoraBanco promotoraBanco)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            var promotora = _promotorasRepositorio.BuscarPromotoraPorId(promotoraBanco.PromotoraId);

            if (!_permissaoService.UsuarioTemAcessoPromotora(usuarioLogado, promotora))
            {
                TempData["MensagemErro"] =
                   "Você não possui acesso.";

                return RedirectToAction("Index", "Promotora");
            }

            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Não foi possivel salvar o login.";
                ViewBag.promotoraId = promotoraBanco.PromotoraId;
                ViewBag.NomePromotora = promotora.Nome;
                ViewBag.Banco = _bancosRepositorio.ListarBancos();
                ViewBag.Usuario = _usuarioRepositorio.ListarTodosUsuarios();

                return View(promotoraBanco);
            }

            _promotoraBancoRepositorio.Adicionar(promotoraBanco);
            TempData["MensagemSucesso"] = "Login adicionado com sucesso";
            return RedirectToAction("Index", new { promotoraId = promotoraBanco.PromotoraId });

        }

        [PaginaRestritaSomenteAdmin]
        public IActionResult Editar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            var promotoraBanco = _promotoraBancoRepositorio.BuscarPorId(id);

            if (!_permissaoService.UsuarioTemAcessoPromotoraBanco(usuarioLogado, promotoraBanco))
            {
                TempData["MensagemErro"] =
                    "Você não possui acesso.";

                return RedirectToAction("Index", "PromotoraBanco");
            }

            ViewBag.Banco = _bancosRepositorio.ListarBancos();
            ViewBag.Usuario = _usuarioRepositorio.ListarTodosUsuarios();



            if (promotoraBanco == null)
            {
                TempData["MensagemErro"] = "Login não localizado";
                return RedirectToAction("Index");
            }

            ViewBag.NomePromotora = promotoraBanco.Promotora.Nome;
            ViewBag.PromotoraId = promotoraBanco.PromotoraId;

            return View(promotoraBanco);
        }

        [PaginaRestritaSomenteAdmin]
        [HttpPost]
        public IActionResult Editar(PromotoraBanco promotoraBanco)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            var promotoraBancoDb = _promotoraBancoRepositorio.BuscarPorId(promotoraBanco.Id);

            if (!_permissaoService.UsuarioTemAcessoPromotoraBanco(usuarioLogado, promotoraBancoDb))
            {
                TempData["MensagemErro"] = "Você não possui acesso";

                return RedirectToAction("Index", "PromotoraBanco");
            }

            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Não foi possivel alterar o login";
                ViewBag.Banco = _bancosRepositorio.ListarBancos();
                ViewBag.Usuario = _usuarioRepositorio.ListarTodosUsuarios();
                ViewBag.PromotoraId = promotoraBancoDb.PromotoraId;
                ViewBag.NomePromotora = _promotorasRepositorio.BuscarPromotoraPorId(promotoraBancoDb.PromotoraId).Nome;
                return View(promotoraBanco);
            }

            promotoraBancoDb.Login = promotoraBanco.Login;
            promotoraBancoDb.Senha = promotoraBanco.Senha;
            promotoraBancoDb.BancoId = promotoraBanco.BancoId;
            promotoraBancoDb.UsuarioId = promotoraBanco.UsuarioId;

            _promotoraBancoRepositorio.Atualizar(promotoraBancoDb);
            TempData["MensagemSucesso"] = "Login atualizado com sucesso!";
            return RedirectToAction("Index", new { promotoraId = promotoraBancoDb.PromotoraId });

        }

        [PaginaRestritaSomenteAdmin]
        public IActionResult Excluir(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;


            var promotoraBanco = _promotoraBancoRepositorio.BuscarPorId(id);

            if(!_permissaoService.UsuarioTemAcessoPromotoraBanco(usuarioLogado, promotoraBanco))
            {
                TempData["MensagemErro"] = "Você não possui acesso";

                return RedirectToAction("Index", "PromotoraBanco");
            }

            if (promotoraBanco == null)
            {
                TempData["MensagemErro"] = "Login não localizado.";
                return RedirectToAction("Index");
            }

            return View(promotoraBanco);
        }

        [PaginaRestritaSomenteAdmin]
        [HttpPost]
        public IActionResult Apagar(int id)
        {

            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            try
            {
                var promotoraBanco = _promotoraBancoRepositorio.BuscarPorId(id);

                if(!_permissaoService.UsuarioTemAcessoPromotoraBanco(usuarioLogado, promotoraBanco))
                {
                    TempData["MensagemErro"] = "Você não possui acesso";

                    return RedirectToAction("Index", "PromotoraBanco");

                }

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
