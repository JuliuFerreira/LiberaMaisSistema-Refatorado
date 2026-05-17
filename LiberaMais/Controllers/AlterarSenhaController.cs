using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    public class AlterarSenhaController : Controller
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly ISessao _Sessao;
        public AlterarSenhaController(IUsuarioRepositorio usuarioRepositorio, ISessao sessao)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _Sessao = sessao;
        }

        public IActionResult index()
        {
            return View();
        }

        [HttpPost]

        public IActionResult Alterar(AlterarSenha alterarSenha)
        {
            try
            {
                UsuarioModel usuarioLogado = _Sessao.BuscarSessaoDoUsuario();
                alterarSenha.Id = usuarioLogado.Id;

                if(ModelState.IsValid)
                {
                    _usuarioRepositorio.AlterarSenha(alterarSenha);
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Dados inválidos, por favor, tente novamente." });
            }
            catch (Exception erro)
            {
                // Erro inesperado
                return Json(new { success = false, message = $"Não foi possível alterar sua senha, tente novamente. Detalhe do erro: {erro.Message}" });
            }
        }
    }
}
