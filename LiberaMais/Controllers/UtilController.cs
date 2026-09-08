using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    public class UtilController : Controller
    {
        private readonly IUtilRepositorio _UtilRepositorio;

        public UtilController(IUtilRepositorio utilRepositorio)
        {
            _UtilRepositorio = utilRepositorio;
        }

        public IActionResult Index(string nome, int pagina = 1)
        {
            if (pagina < 1)
            {
                pagina = 1;
            }

            const int tamanhoPagina = 10;
            int totalRegistros;

            var listaUtils = _UtilRepositorio.BuscarPorNome(
                nome,
                pagina,
                tamanhoPagina,
                out totalRegistros);

            ViewBag.NomeAtual = nome;
            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanhoPagina);

            return View(listaUtils);
        }
        public IActionResult Criar()
        {

            return View();

        }

        [HttpPost]
        public IActionResult Criar(Util util)
        {

            if (ModelState.IsValid)
            {
                _UtilRepositorio.Adicionar(util);
                TempData["MensagemSucesso"] = "Link adicionado com sucesso.";
                return RedirectToAction("Index");
            }

            TempData["MensagemErro"] = "Não é possivel adicionar esse link";

            return View(util);

        }

        public IActionResult Editar(int id)
        {

            var util = _UtilRepositorio.BuscarPorId(id);

            if (util == null)
            {
                TempData["MensagemErro"] = "Link não localizado";
                return RedirectToAction("Index");
            }

            return View(util);

        }

        [HttpPost]
        public IActionResult Editar(Util util)
        {

            if (util.Nome == null)
            {
                TempData["MensagemErro"] = "Você precisa informar um nome.";
                return View(util);
            }

            if (util.Url == null)
            {
                TempData["MensagemErro"] = "Você precisa informar a URL do site.";
                return View(util);
            }

            var utilDb = _UtilRepositorio.BuscarPorId(util.Id);

            if(utilDb == null)
            {
                TempData["MensagemErro"] = "Não foi possível efetuar a edição";
                return View(util);
            }


            utilDb.Nome = util.Nome;
            utilDb.Login = util.Login;
            utilDb.Senha = util.Senha;
            utilDb.Url = util.Url;
            utilDb.Descricao = util.Descricao;

            _UtilRepositorio.Atualizar(utilDb);
            TempData["MensagemSucesso"] = "Atualização realizada com sucesso!";

            return RedirectToAction("Index");
        }

        public IActionResult Deletar(int id)
        {
            var util = _UtilRepositorio.BuscarPorId(id);

            if (util == null)
            {
                TempData["MensagemErro"] = "Link não localizado";
                return RedirectToAction("Index");
            }

            return View(util);
        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            try
            {
                bool apagado = _UtilRepositorio.Apagar(id);
                        
                if (apagado)
                {
                    TempData["MensagemSucesso"] = "Link removido com sucesso!";
                    return RedirectToAction("Index");
                }

                else
                {
                    TempData["MensagemErro"] = "link não removido.";

                }
            }

            catch(Exception)
            {
                TempData["MensagemErro"] = "Não foi possível remover o link cadastrado.";
                return RedirectToAction("Index");

            }

            return RedirectToAction("Index");

        }

    }
}
