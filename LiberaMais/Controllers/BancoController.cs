using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Controllers
{

    [PaginaParaUsuarioLogado]
    public class BancoController : Controller
    {
        private readonly IBancosRepositorio _bancosRepositorio;
        private readonly ISessao _sessao;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public BancoController(IBancosRepositorio bancosRepositorio, IUsuarioRepositorio usuarioRepositorio, ISessao sessaoRepositorio)
        {
            _bancosRepositorio = bancosRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _sessao = sessaoRepositorio;
        }

        public IActionResult Index()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            List<Banco> bancos = _bancosRepositorio.ListarBancos();
            return View(bancos);
        }


        public IActionResult Criar()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;


            return View();
        }


        [HttpPost]
        public IActionResult Criar(Banco bancos)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            try
            {
                if (ModelState.IsValid)
                {
                    _bancosRepositorio.Adicionar(bancos);
                    TempData["MensagemSucesso"] = "Banco cadastrado com sucesso!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["MensagemErro"] = "Erro ao cadastrar o banco.";
                }
                return View(bancos);

            }
            catch (Exception)
            {
                TempData["MensagemErro"] = "Ops, não foi possível cadastrar o banco!";
                return View(bancos);
            }

        }

        public IActionResult Editar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            var banco = _bancosRepositorio.BuscarBancoPorId(id);

            if (banco == null)
            {
                return RedirectToAction("Index");
            }

            return View(banco);
        }


        [HttpPost]
        public IActionResult Editar(Banco bancos)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["MensagemErro"] = "Alteração não realizada.";
                    return View(bancos);
                }

                _bancosRepositorio.Atualizar(bancos);
                TempData["MensagemSucesso"] = "Alteração realizada com sucesso!";
                return RedirectToAction("Index");

            }
            catch (Exception)
            {
                TempData["MensagemErro"] = "Ops, não foi possível alterar o banco!";
                return View(bancos);
            }

        }

        public IActionResult Excluir(int id)
        {
            var banco = _bancosRepositorio.BuscarBancoPorId(id);

            if(banco == null)
            {
                TempData["MensagemErro"] = "Banco não localizado.";
                return RedirectToAction("Index");
            }

            return View(banco);

        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {

            try
            {
                bool apagado = _bancosRepositorio.Apagar(id);

                if (apagado)
                {
                    TempData["MensagemSucesso"] = "Banco excluido com sucesso!";
                    return RedirectToAction("Index");
                }

                else
                {
                    TempData["MensagemErro"] = "Banco não excluido!";
                    return RedirectToAction("Index");
                }
            }

            catch (Exception)
            {
                TempData["MensagemErro"] = "Ops, não foi possível excluir o banco!";

                return RedirectToAction("Index");

            }

        }

    }
}
