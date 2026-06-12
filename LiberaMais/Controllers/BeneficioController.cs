using LiberaMais.Filters;
using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class BeneficioController : Controller
    {
        private readonly IBeneficioRepositorio _BeneficioRepositorio;
        private readonly IOrgaoRepositorio _OrgaoRepositorio;

        public BeneficioController(IBeneficioRepositorio beneficioRepositorio, IOrgaoRepositorio orgaoRepositorio)
        {
            _BeneficioRepositorio = beneficioRepositorio;
            _OrgaoRepositorio = orgaoRepositorio;
        }

        public IActionResult Index()
        {

            var beneficio = _BeneficioRepositorio.ListarTodos();

            return View(beneficio);
        }

        public IActionResult Criar()
        {
            ViewBag.orgao = _OrgaoRepositorio.ListarTodos();
            return View();
        }

        [HttpPost]
        public IActionResult Criar (Beneficio beneficio)
        {
            ViewBag.orgao = _OrgaoRepositorio.ListarTodos();


            if (ModelState.IsValid)
            {
                beneficio.Descricao = beneficio.Descricao.ToUpper();
                _BeneficioRepositorio.Adicionar(beneficio);
                TempData["MensagemSucesso"] = "Beneficio adicionado com sucesso.";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["MensagemErro"] = "Não foi possível adicionar o beneficio.";
            }

            return View(beneficio);
        }

        public IActionResult Editar (int id)
        {
            ViewBag.orgao = _OrgaoRepositorio.ListarTodos();
            var beneficio = _BeneficioRepositorio.BuscarPorId(id);

            if(beneficio == null)
            {
                TempData["MensagemErro"] = "Beneficio não encontrado.";
                return RedirectToAction("Index");
            }

            return View(beneficio);
        }

        [HttpPost]
        public IActionResult Editar(Beneficio beneficio)
        {
            var DbBeenficio = _BeneficioRepositorio.BuscarPorId(beneficio.Id);

          ViewBag.orgao = _OrgaoRepositorio.ListarTodos();

            if (DbBeenficio == null)
            {
                TempData["MensagemErro"] = "Benefício não encontrado.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.orgao = _OrgaoRepositorio.ListarTodos();
                TempData["MensagemErro"] = "Não é possivel editar o benenficio.";
                return View(beneficio);
            }

            DbBeenficio.Codigo = beneficio.Codigo;
            DbBeenficio.Descricao = beneficio.Descricao.ToUpper();

            _BeneficioRepositorio.Atualizar(DbBeenficio);
            TempData["MensagemSucesso"] = "Beneficio atualizado com sucesso.";
            return RedirectToAction("Index");
        }

        public IActionResult Deletar(int id)
        {
            ViewBag.orgao = _OrgaoRepositorio.ListarTodos();

            var beneficio = _BeneficioRepositorio.BuscarPorId(id);

            if (beneficio == null)
            {
                TempData["MensagemErro"] = "Beneficio não encontrado.";
                return RedirectToAction("Index");
            }

            return View(beneficio);

        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            try
            {
                var apagado = _BeneficioRepositorio.Apagar(id);

                if (apagado)
                {
                    TempData["MensagemSucesso"] = "Beneficio excluido com sucesso.";
                    return RedirectToAction("Index");
                }

                else
                {
                    TempData["MensagemErro"] = "Beneficio não excluido.";
                    return RedirectToAction("Index");

                }
            }

            catch (Exception)
            {
                TempData["MensagemErro"] = "Não foi possivel excluir esse benenficio.";                
            }

            return RedirectToAction("Index");
        }
    }
}
