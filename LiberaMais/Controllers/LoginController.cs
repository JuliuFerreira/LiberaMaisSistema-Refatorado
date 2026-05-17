using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LiberaMais.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly ISessao _sessao;
        private readonly IEmail _email;

        public LoginController(IUsuarioRepositorio usuarioRepositorio,
                                ISessao sessao,
                                IEmail email)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _sessao = sessao;
            _email = email;
        }

        public IActionResult Index()
        {

            //se o usuario estive logado, redirecionar para a home.

            if (_sessao.BuscarSessaoDoUsuario() != null) return RedirectToAction("Index", "Apps");

            return View();
        }

        [HttpPost]

        public IActionResult Entrar(LoginModel loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UsuarioModel usuario = _usuarioRepositorio.BuscarPorLogin(loginModel.Login);

                    if (usuario != null)
                    {
                        if (usuario.SenhaValida(loginModel.Senha))
                        {
                            _sessao.CriarSessaoDoUsuario(usuario);
                            return Json(new { success = true });
                        }
                        else
                        {
                            // Senha inválida
                            return Json(new { success = false, message = "Senha inválida, por favor, tente novamente." });
                        }
                    }
                    else
                    {
                        // Usuário não encontrado
                        return Json(new { success = false, message = "Usuário não encontrado, por favor, tente novamente." });
                    }
                }

                return Json(new { success = false, message = "Dados inválidos, por favor, tente novamente." });
            }
            catch (Exception erro)
            {
                // Erro inesperado
                return Json(new { success = false, message = $"Não foi possível realizar seu login, tente novamente. Detalhe do erro: {erro.Message}" });
            }
        }


        [HttpPost]
        public IActionResult LinkParaRedefinirSenha(RedefinirSenha redefinirSenha)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UsuarioModel usuario = _usuarioRepositorio.BuscarPorEmailELogin(redefinirSenha.Email, redefinirSenha.Login);

                    if (usuario != null)
                    {
                        string novaSenha = usuario.GerarNovaSenha();
                        string mensagem = $"Olá {usuario.Nome}, sua nova senha para acesso ao sistema é: {novaSenha}, sugerimos que troque a senha informada quando acessar o sistema.";

                        bool emailEnviado = _email.Enviar(usuario.Email, "Sistema Libera Mais - Nova senha", mensagem);

                        if (emailEnviado)
                        {
                            _usuarioRepositorio.Atualizar(usuario);
                            return Json(new { success = true, message = "Enviamos uma nova senha para seu e-mail." });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Não conseguimos enviar o email, por favor, tente novamente." });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Não conseguimos redefinir sua senha. Por favor, verifique os dados informados." });
                    }
                }

                return Json(new { success = false, message = "Dados inválidos. Verifique os campos e tente novamente." });
            }
            catch (Exception erro)
            {
                return Json(new { success = false, message = $"Não conseguimos redefinir sua senha, confira seu login e e-mail e tente novamente, detalhe do erro: {erro.Message}" });
            }
        }


        public ActionResult RedefinirSenha()
        {
            return View();
        }


        public IActionResult Sair()
        {
            _sessao.RemoverSessaoDoUsuario();
            return RedirectToAction("Index", "Login");
        }
    }
}
