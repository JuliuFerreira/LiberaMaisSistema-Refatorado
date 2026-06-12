using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class UsuarioController : Controller
    {

        private readonly IUsuarioRepositorio _usuarioRepositorio; ISessao _sessao;
        public UsuarioController(IUsuarioRepositorio usuarioRepositorio, ISessao sessao)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _sessao = sessao;
        }
        public IActionResult Index()
        {

            List<UsuarioModel> usuarios = _usuarioRepositorio.BuscarTodos();

            return View(usuarios);
        }

        public IActionResult Criar()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Criar(UsuarioModel usuario)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    usuario = _usuarioRepositorio.Adicionar(usuario);
                    return Json(new { success = true, message = "Usuário cadastrado com sucesso!" });

                }
                else
                {
                    return Json(new { success = false, message = "Ops, não foi possível cadastrar o usuário!" });
                }
            }

            catch ( System.Exception erro) 
            {
                return Json(new { success = false, message = $"Ops, não foi possível cadastrar o usuário! Detalhe do erro: {erro.Message}" });
            }

        }

        public IActionResult Editar(int id)
        {
            UsuarioModel usuario = _usuarioRepositorio.BuscarPorId(id);
            return View(usuario);
        }

        [HttpPost]
        public IActionResult Editar(UsuarioModel usuario)
        {
            try
            {

                if (!ModelState.IsValid)
                {               
                    usuario = _usuarioRepositorio.Atualizar(usuario);
                    return Json(new { success = true, message = "Alteração realizada com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Ops, não foi possível alterar o usuário!" });
                }
            }

            catch (System.Exception erro) 
            {
                return Json(new { success = false, message = $"Ops, não foi possível possível editar o usuário! Detalhe do erro: {erro.Message}" });
            }
        }

        public IActionResult ApagarConfirmacao(int id)
        {
            var usuario = _usuarioRepositorio.BuscarPorId(id);
            return View (usuario);
        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            try
            {
                bool apagado = _usuarioRepositorio.Apagar(id);
                                          

                if (apagado)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Não foi possível excluir o usuário." });
                }
            }
            catch (Exception erro)
            {
                return Json(new { success = false, message = $"Não foi possível excluir o usuário. Detalhes do erro: {erro.Message}" });
            }
        }
    }
}
