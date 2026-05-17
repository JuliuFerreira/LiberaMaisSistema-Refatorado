using LiberaMais.Filters;
using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class DespesaController : Controller
    {
        private readonly IDespesaRepositorio _despesaRepositorio;
        private readonly IFinancaRepositorio _financaRepositorio;

        public DespesaController(IDespesaRepositorio despesaRepositorio, IFinancaRepositorio financaRepositorio)
        {
            _despesaRepositorio = despesaRepositorio;
            _financaRepositorio = financaRepositorio;
        }

        public IActionResult Index(int idFinanca)
        {
            ViewBag.idFinanca = idFinanca;

            var financa = _financaRepositorio.BuscarMesAnoPorId(idFinanca);

            var despesaRepositorio = _despesaRepositorio.ListarDespesas(idFinanca);

            ViewBag.financa = financa;

            return View(despesaRepositorio);
        }

        public IActionResult CriarDespesa(int idFinanca)
        {
            var despesa = new Despesa()
            {
                FinancaId = idFinanca,
            };
            return View(despesa);
        }

        [HttpPost]
        public IActionResult CriarDespesa(Despesa despesa)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Essa despesa não pode ser cadastrada!" });
                }

                _despesaRepositorio.Adicionar(despesa);
                return Json(new { success = true });
            }

            catch (System.Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível cadastrar a nova despesa! Detalhe do erro: {erro.Message}" });
            }

        }

        public IActionResult EditarDespesa(int id)
        {
            var despesa = _despesaRepositorio.BuscarDespesaPorId(id);

            if (despesa == null)
            {
                RedirectToAction("Index");
            }

            return View(despesa);
        }
        [HttpPost]
        public IActionResult EditarDespesa(Despesa despesa)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Essa despesa não pode ser editada!" });
                }

                _despesaRepositorio.Atualizar(despesa);
                return Json(new { success = true });
            }
            catch (Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível editar essa despesa! Detalhe do erro: {erro.Message}" });
            }
        }

        public IActionResult ExcluirDespesa(int id)
        {
            var del = _despesaRepositorio.BuscarDespesaPorId(id);
            return View(del);
        }
        [HttpPost]
        public IActionResult Apagar(int id)
        {
            try
            {
                var banco = _despesaRepositorio.BuscarDespesaPorId(id);
                if (banco == null)
                {
                    return Json(new { success = false, message = "Despesa excluida com sucesso!" });
                }

                bool del = _despesaRepositorio.Apagar(id);

                if (del)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Ops, não foi possível excluir essa despesa." });
                }
            }
            catch (Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível excluir a despesa. Detalhes do erro: {erro.Message}" });
            }
        }
    }
}

