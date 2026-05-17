using LiberaMais.Filters;
using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class ReceitaController : Controller
    {
        private readonly IReceitaRepositorio _receitaRepositorio;
        private readonly IFinancaRepositorio _financaRepositorio;

        public ReceitaController(IReceitaRepositorio receitaRepositorio, IFinancaRepositorio financaRepositorio)
        {
            _receitaRepositorio = receitaRepositorio;
            _financaRepositorio = financaRepositorio;
        }

        public IActionResult Index(int idFinanca)
        {
            ViewBag.idFinanca = idFinanca;

            var financa = _financaRepositorio.BuscarMesAnoPorId(idFinanca);

            var receitaRepositorio = _receitaRepositorio.ListarReceitas(idFinanca);




            ViewBag.financa = financa;

            return View(receitaRepositorio);
        }

        public IActionResult CriarReceita(int idFinanca)
        {
            var receita = new Receita()
            {
                FinancaId = idFinanca,
            };
            return View(receita);
        }

        [HttpPost]
        public IActionResult CriarReceita(Receita receita)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Essa receita não pode ser cadastrada!" });
                }

                _receitaRepositorio.Adicionar(receita);
                return Json(new { success = true });
            }

            catch (System.Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível cadastrar a nova receita! Detalhe do erro: {erro.Message}" });
            }

        }

        public IActionResult EditarReceita(int id)
        {
            var receita = _receitaRepositorio.BuscarReceitaPorId(id);

            if (receita == null)
            {
                RedirectToAction("Index");
            }

            return View(receita);
        }
        [HttpPost]
        public IActionResult EditarReceita(Receita receita)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Essa receita não pode ser editada!" });
                }

                _receitaRepositorio.Atualizar(receita);
                return Json(new { success = true });
            }
            catch (Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível editar essa receita! Detalhe do erro: {erro.Message}" });
            }
        }

        public IActionResult ExcluirReceita(int id)
        {
            var del = _receitaRepositorio.BuscarReceitaPorId(id);
            return View(del);
        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            try
            {
                var banco = _receitaRepositorio.BuscarReceitaPorId(id);
                if (banco == null)
                {
                    return Json(new { success = false, message = "Receita excluida com sucesso!" });
                }

                bool del = _receitaRepositorio.Apagar(id);

                if (del)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Ops, não foi possível excluir essa receita." });
                }
            }
            catch (Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível excluir a receita. Detalhes do erro: {erro.Message}" });
            }
        }
    }
}
