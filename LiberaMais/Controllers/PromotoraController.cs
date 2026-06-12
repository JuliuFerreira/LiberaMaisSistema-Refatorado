using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using LiberaMais.Services;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace LiberaMais.Controllers
{

    [PaginaParaUsuarioLogado]
    public class PromotoraController : Controller
    {
        private readonly IPromotorasRepositorio _promotorasRepositorio;
        private readonly IPromotoraBancoRepositorio _promotoraBancoRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly ISessao _sessao;
        private readonly PermissaoService _permissaoService;

        public PromotoraController(IPromotorasRepositorio promotorasRepositorio, ISessao sessao, IUsuarioRepositorio usuarioRepositorio, IPromotoraBancoRepositorio promotoraBancoRepositorio, PermissaoService permissaoService)
        {
            _promotorasRepositorio = promotorasRepositorio;
            _sessao = sessao;
            _usuarioRepositorio = usuarioRepositorio;
            _promotoraBancoRepositorio = promotoraBancoRepositorio;
            _permissaoService = permissaoService;
        }

        public IActionResult Index()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;


            List<Promotora> promotora;


            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                promotora =
                    _promotorasRepositorio.ListarPromotora();
            }
            else
            {
                promotora =
                    _promotorasRepositorio
                        .ListarPorUsuario(usuarioLogado.Id);
            }

            return View(promotora);
        }

        [PaginaRestritaSomenteAdmin]
        public IActionResult Criar()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios =
                    _usuarioRepositorio.ListarTodosUsuarios();
            }
            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
                {
                    usuarioLogado

                };

            }

            return View();
        }

        [PaginaRestritaSomenteAdmin]
        [HttpPost]
        public IActionResult Criar(Promotora promotora)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            try
            {

                if (usuarioLogado.Perfil == PerfilEnum.Admin)
                {
                    ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
                }
                else
                {
                    ViewBag.Usuarios = new List<UsuarioModel>
                {
                    usuarioLogado

                };

                }

                if (ModelState.IsValid)
                {
                    _promotorasRepositorio.Adicionar(promotora);
                    TempData["MensagemSucesso"] = "Promotora adicionada com sucesso!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["MensagemErro"] = "Promotora não adicionada.";
                    return View(promotora);
                }


            }
            catch (Exception)
            {
                if (usuarioLogado.Perfil == PerfilEnum.Admin)
                {
                    ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
                }
                else
                {
                    TempData["MensagemErro"] = "Não foi possivel adicionar a promotora!";
                    ViewBag.Usuarios = new List<UsuarioModel>
                {
                    usuarioLogado

                };

                }

                return View(promotora);
            }
        }

        [PaginaRestritaSomenteAdmin]
        public IActionResult Editar(int id)
        {

            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            var promotora = _promotorasRepositorio.BuscarPromotoraPorId(id);


            if (!_permissaoService.UsuarioTemAcessoPromotora(usuarioLogado, promotora))
            {
                TempData["mensagemErro"] = "Você não tem autorização";

                return RedirectToAction("Index", "Promotora");
            }

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
             
               ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            }
            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
                {
                    usuarioLogado
                };
            }

            return View(promotora);
        }

        [PaginaRestritaSomenteAdmin]
        [HttpPost]
        public IActionResult Editar(Promotora promotora)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            var promotoraDb = _promotorasRepositorio.BuscarPromotoraPorId(promotora.Id);

            try
            {
                if (!_permissaoService.UsuarioTemAcessoPromotora(usuarioLogado, promotoraDb))
                {
                    TempData["mensagemErro"] = "Você não tem permissão";

                    return RedirectToAction("Index", "Promotora");
                }

                if (usuarioLogado.Perfil == PerfilEnum.Admin)
                {
                    ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
                }

                else
                {
                    ViewBag.Usuarios = new List<UsuarioModel>
                    {
                        usuarioLogado
                    };
                }

                if (ModelState.IsValid)
                {
                    promotoraDb.Nome = promotora.Nome;
                    promotoraDb.Login = promotora.Login;
                    promotoraDb.Senha = promotora.Senha;
                    promotoraDb.Url = promotora.Url;

                    if (usuarioLogado.Perfil == PerfilEnum.Admin)
                    {
                        promotoraDb.UsuarioId = promotora.UsuarioId;
                    }

                    _promotorasRepositorio.Atualizar(promotoraDb);

                    TempData["MensagemSucesso"] = "Promotora editada com sucesso!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["MensagemErro"] = "Promotora não editada.";
                    if (usuarioLogado.Perfil == PerfilEnum.Admin)
                    {
                        ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
                    }

                    else
                    {
                        ViewBag.Usuarios = new List<UsuarioModel>
                    {
                        usuarioLogado
                    };
                    }
                    return View(promotora);
                }

            }
            catch (Exception)
            {
                TempData["MensagemErro"] = "Não foi possivel editar a promotora!";
                return View(promotora);
            }
        }

        [PaginaRestritaSomenteAdmin]
        public IActionResult Deletar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            var promotora = _promotorasRepositorio.BuscarPromotoraPorId(id);

            if (!_permissaoService.UsuarioTemAcessoPromotora(usuarioLogado, promotora))
            {
                TempData["mensagemErro"] = "Você não tem permissão";

                return RedirectToAction("Index", "Promotora");
            }

            return View(promotora);
        }


        [PaginaRestritaSomenteAdmin]
        [HttpPost]
        public IActionResult Apagar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            var promotora = _promotorasRepositorio.BuscarPromotoraPorId(id);

            try
            {
                var login = _promotoraBancoRepositorio.ListarPorPromotora(id);

                if (!_permissaoService.UsuarioTemAcessoPromotora(usuarioLogado, promotora))
                {
                    TempData["mensagemErro"] = "Você não tem permissão";

                    return RedirectToAction("Index", "Promotora");
                }

                if (login.Any())
                {
                    TempData["MensagemErro"] = "Não é possível excluir essa promotora, pois existem bancos cadastrados.";

                    return RedirectToAction("Index");
                }

                bool apagado = _promotorasRepositorio.Apagar(id);

                if (apagado)
                {
                    TempData["MensagemSucesso"] = "Promotora Excluida com sucesso!";

                    return RedirectToAction("Index");
                }

                else
                {
                    TempData["MensagemErro"] = "Promotora não excluida.";
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["MensagemErro"] = "Não foi possível excluir a promotora!";
                return RedirectToAction("Index");
            }
        }

    }

}
