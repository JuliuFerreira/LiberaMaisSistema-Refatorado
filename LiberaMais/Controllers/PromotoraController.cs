using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
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

        public PromotoraController(IPromotorasRepositorio promotorasRepositorio, ISessao sessao, IUsuarioRepositorio usuarioRepositorio, IPromotoraBancoRepositorio promotoraBancoRepositorio)
        {
            _promotorasRepositorio = promotorasRepositorio;
            _sessao = sessao;
            _usuarioRepositorio = usuarioRepositorio;
            _promotoraBancoRepositorio = promotoraBancoRepositorio;
        }

        public IActionResult Index()
        {
            var usuarioLogado =
                _sessao.BuscarSessaoDoUsuario();

            ViewBag.IsAdmin =
                usuarioLogado.Perfil == PerfilEnum.Admin;

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

        public IActionResult Editar(int id)
        {
            var edit = _promotorasRepositorio.BuscarPromotoraPorId(id);
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if(usuarioLogado.Perfil == PerfilEnum.Admin)
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

                return View(edit);
        }


        [HttpPost]
        public IActionResult Editar(Promotora promotora)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            try
            {
                if(usuarioLogado.Perfil == PerfilEnum.Admin)
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
                    _promotorasRepositorio.Atualizar(promotora);
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
        public IActionResult Deletar(int id)
        {
            var del = _promotorasRepositorio.BuscarPromotoraPorId(id);

            return View(del);
        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {


            try
            {
                var login = _promotoraBancoRepositorio.ListarPorPromotora(id);

                if (login.Any())
                {
                    TempData["MensagemErro"] =
                "Não é possível excluir essa promotora, pois existem bancos cadastrados.";

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
