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
                    // 💡 A MÁGICA ACONTECE AQUI: Tratamos o nome para salvar apenas as primeiras maiúsculas
                    string nomeMinusculo = usuario.Nome.Trim().ToLower();
                    usuario.Nome = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nomeMinusculo);

                    // 1. Buscamos no banco se já existe alguém com o mesmo NOME
                    // (Agora comparamos usando o nome que já foi tratado acima)
                    var usuarioExiste = _usuarioRepositorio.BuscarPorNome(usuario.Nome);

                    if (usuarioExiste != null)
                    {
                        TempData["MensagemErro"] = "Já existe um usuário cadastrado com esse nome.";
                        return View(usuario);
                    }

                    // 2. Agora sim! O 'usuario.Nome' vai salvo formatado como "Nome Sobrenome"
                    _usuarioRepositorio.Adicionar(usuario);
                    TempData["MensagemSucesso"] = "Usuário cadastrado com sucesso!";
                    return RedirectToAction("Index");
                }

                return View(usuario);
            }
            catch (Exception erro)
            {
                TempData["MensagemErro"] = $"Ops, não conseguimos cadastrar o usuário. Detalhe: {erro.Message}";
                return RedirectToAction("Index");
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
                if (ModelState.IsValid)
                {
                    // 1. Tratamos o nome para salvar apenas as primeiras maiúsculas (Title Case)
                    string nomeMinusculo = usuario.Nome.Trim().ToLower();
                    usuario.Nome = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nomeMinusculo);

                    // 2. Buscamos se JÁ EXISTE OUTRO usuário com esse mesmo nome (ignorando o ID atual)
                    var usuarioExiste = _usuarioRepositorio.BuscarPorNome(usuario.Nome);

                    // Se encontrou um usuário com o mesmo nome, mas com ID diferente, barra a edição
                    if (usuarioExiste != null && usuarioExiste.Id != usuario.Id)
                    {
                        TempData["MensagemErro"] = "Já existe outro usuário cadastrado com esse nome.";
                        return View(usuario);
                    }

                    // 3. Se passou na validação, atualiza o registro
                    _usuarioRepositorio.Atualizar(usuario);
                    TempData["MensagemSucesso"] = "Usuário atualizado com sucesso!";
                    return RedirectToAction("Index");
                }

                return View(usuario);
            }
            catch (Exception erro)
            {
                TempData["MensagemErro"] = $"Ops, não conseguimos atualizar o usuário. Detalhe: {erro.Message}";
                return RedirectToAction("Index");
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
                    TempData["MensagemSucesso"] = "Usuário excluido com sucesso!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["MensagemErro"] = "Usuário não excluido.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception erro)
            {
                TempData["MensagemErro"] = "Não foi possível excluir esse usuário.";
                return RedirectToAction("Index");
            }
        }
    }
}
