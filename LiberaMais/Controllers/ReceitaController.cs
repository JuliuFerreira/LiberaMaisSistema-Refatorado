using LiberaMais.Filters;
using LiberaMais.Models;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LiberaMais.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class ReceitaController : Controller
    {
        private readonly IReceitaRepositorio _receitaRepositorio;
        private readonly IFinancaRepositorio _financaRepositorio;
        private readonly IPromotorasRepositorio _promotoraRepositorio;

        public ReceitaController(IReceitaRepositorio receitaRepositorio, IFinancaRepositorio financaRepositorio, IPromotorasRepositorio promotoraRepositorio)
        {
            _receitaRepositorio = receitaRepositorio;
            _financaRepositorio = financaRepositorio;
            _promotoraRepositorio = promotoraRepositorio;
        }


        public IActionResult Index(int idFinanca)
        {
            ViewBag.idFinanca = idFinanca;

            var financa = _financaRepositorio.BuscarMesAnoPorId(idFinanca);
            var receitaRepositorio = _receitaRepositorio.ListarReceitas(idFinanca);

            // Armazena na ViewBag o objeto da finança mãe
            ViewBag.financa = financa;

            return View(receitaRepositorio);
        }

        [HttpGet]
        public IActionResult CriarReceita(int idFinanca)
        {
            var promotoras = _promotoraRepositorio.ListarPromotora();
            ViewBag.Promotoras = new SelectList(promotoras, "Id", "Nome");

            var receita = new Receita()
            {
                FinancaId = idFinanca,
                DataReceita = DateTime.Today
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
                    // Crucial: Recarregar a lista se o modelo for inválido para evitar dropdown vazio
                    var promotoras = _promotoraRepositorio.ListarPromotora();
                    ViewBag.Promotoras = new SelectList(promotoras, "Id", "Nome");

                    TempData["mensagemErro"] = "Essa receita não pode ser cadastrada! Verifique os dados.";
                    return View(receita);
                }

                _receitaRepositorio.Adicionar(receita);
                TempData["mensagemSucesso"] = "Receita cadastrada com sucesso!";

                return RedirectToAction("Index", new { idFinanca = receita.FinancaId });
            }
            catch (Exception erro)
            {
                var promotoras = _promotoraRepositorio.ListarPromotora();
                ViewBag.Promotoras = new SelectList(promotoras, "Id", "Nome");

                // PEGA O ERRO INTERNO REAL
                string mensagemReal = erro.InnerException != null ? erro.InnerException.Message : erro.Message;

                TempData["mensagemErro"] = $"Erro real do Banco: {mensagemReal}";
                return View(receita);
            }
        }

        [HttpGet] // Adicionado explicitamente para manter o padrão
        public IActionResult EditarReceita(int id)
        {
            var receita = _receitaRepositorio.BuscarReceitaPorId(id);

            if (receita == null)
            {
                TempData["mensagemErro"] = "Receita não encontrada.";
                return RedirectToAction("Index", "Financa");
            }

            var promotoras = _promotoraRepositorio.ListarPromotora();
            ViewBag.Promotoras = new SelectList(promotoras, "Id", "Nome", receita.PromotoraId);

            return View(receita);
        }

        [HttpPost]
        public IActionResult EditarReceita(Receita receita)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var promotoras = _promotoraRepositorio.ListarPromotora();
                    ViewBag.Promotoras = new SelectList(promotoras, "Id", "Nome", receita.PromotoraId);

                    TempData["mensagemErro"] = "Essa receita não pode ser editada! Verifique os dados preenchidos.";
                    return View(receita);
                }

                _receitaRepositorio.Atualizar(receita);
                TempData["mensagemSucesso"] = "Receita alterada com sucesso!";

                return RedirectToAction("Index", new { idFinanca = receita.FinancaId });
            }
            catch (Exception erro)
            {
                var promotoras = _promotoraRepositorio.ListarPromotora();
                ViewBag.Promotoras = new SelectList(promotoras, "Id", "Nome", receita.PromotoraId);

                TempData["mensagemErro"] = $"Ops, não foi possível editar essa receita! Detalhe do erro: {erro.Message}";
                return View(receita);
            }
        }

        public IActionResult ExcluirReceita(int id)
        {
            var del = _receitaRepositorio.BuscarReceitaPorId(id);
            if (del == null) return RedirectToAction("Index", "Financa");
            return View(del);
        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            int idFinancaPadrao = 0;
            try
            {
                var banco = _receitaRepositorio.BuscarReceitaPorId(id);
                if (banco == null)
                {
                    TempData["mensagemErro"] = "Receita não localizada no sistema.";
                    return RedirectToAction("Index", "Financa");
                }

                idFinancaPadrao = banco.FinancaId; // Salva o ID antes de apagar para poder retornar à tela certa
                bool del = _receitaRepositorio.Apagar(id);

                if (del)
                {
                    TempData["mensagemSucesso"] = "Receita excluída com sucesso!";
                }
                else
                {
                    TempData["mensagemErro"] = "Ops, não foi possível excluir essa receita.";
                }

                return RedirectToAction("Index", new { idFinanca = idFinancaPadrao });
            }
            catch (Exception erro)
            {
                TempData["mensagemErro"] = $"Ops, não foi possível excluir a receita. Detalhes do erro: {erro.Message}";
                return RedirectToAction("Index", new { idFinanca = idFinancaPadrao });
            }
        }
    }
}

