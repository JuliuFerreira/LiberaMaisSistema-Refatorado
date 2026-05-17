using LiberaMais.Filters;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LiberaMais.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class FinancaController : Controller
    {
        private readonly IFinancaRepositorio _financaRepositorio;

        public FinancaController(IFinancaRepositorio financaRepositorio)
        {
            _financaRepositorio = financaRepositorio;
        }


        public IActionResult FechamentoCaixa(int id)
        {
            Financa financa = _financaRepositorio.BuscarMesAnoPorId(id);

            if (financa == null)
            {
                return RedirectToAction("Index", "Financa");
            }

            int usuarioJulioId = (int)UsuarioEnum.JULIO;
            int usuarioRafaelId = (int)UsuarioEnum.RAFAEL;

            decimal receitasJulio = financa.Receitas
                .Where(r => r.Usuario == usuarioJulioId)
                .Select(r => r.ValorRecebido)
                .DefaultIfEmpty(0)
                .Sum();

            decimal despesasJulio = financa.Despesas
                .Where(d => d.Usuario == usuarioJulioId)
                .Select(d => d.ValorDespesa)
                .DefaultIfEmpty(0)
                .Sum();

            decimal receitasRafael = financa.Receitas
                .Where(r => r.Usuario == usuarioRafaelId)
                .Select(r => r.ValorRecebido)
                .DefaultIfEmpty(0)
                .Sum();

            decimal despesasRafael = financa.Despesas
                .Where(d => d.Usuario == usuarioRafaelId)
                .Select(d => d.ValorDespesa)
                .DefaultIfEmpty(0)
                .Sum();

            var viewModel = new FechamentoCaixaViewModel
            {
                Mes = financa.Mes,
                Ano = financa.Ano,
                ReceitaContaJulio = receitasJulio,
                DespesaContaJulio = despesasJulio,
                ReceitaContaRafael = receitasRafael,
                DespesaContaRafael = despesasRafael
            };

            decimal diferencaParaIgualarSalario = viewModel.Salario - viewModel.SaldoContaJulio;

            if (diferencaParaIgualarSalario < 0)
            {
                // Julio deve repassar a diferença positiva para Rafael
                viewModel.DiferencaSaldoParaIgualarSalario = $"Julio deve repassar {Math.Abs(diferencaParaIgualarSalario).ToString("C2")} para Rafael";
            }
            else if (diferencaParaIgualarSalario > 0)
            {
                // Rafael deve repassar a diferença positiva para Julio
                viewModel.DiferencaSaldoParaIgualarSalario = $"Rafael deve repassar {diferencaParaIgualarSalario.ToString("C2")} para Julio";
            }
            else
            {
                // Saldo já igualado
                viewModel.DiferencaSaldoParaIgualarSalario = "Não há diferença no saldo total entre as contas de Julio e Rafael";
            }

            return View(viewModel);
        }

        public IActionResult Index()
        {

            var financas = _financaRepositorio.ListaFinanca();
            return View(financas);
        }



        public IActionResult AddMesAno()
        {
            ViewBag.Anos = Utils.Utils.GerarAno();
            return View();
        }

        public IActionResult VerificarDuplicidade(int mes, int ano)
        {
            bool exists = _financaRepositorio.ExisteMesEAno(mes, ano);
            return Json(new { exists });
        }

        [HttpPost]
        public IActionResult AddMesAno(Financa financa)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _financaRepositorio.Adicionar(financa);
                    return Json(new { success = true, message = "Mês/ano adicionado com sucesso!" });
                }
                return Json(new { success = false, message = "Dados inválidos." });
            }
            catch (System.Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível criar o mês/ano! Detalhe do erro: {erro.Message}" });
            }
        }

        public IActionResult EditarMesAno(int id)
        {
            var edit = _financaRepositorio.BuscarMesAnoPorId(id);
            ViewBag.Anos = Utils.Utils.GerarAno();
            return View(edit);
        }

        [HttpPost]
        public IActionResult EditarMesAno(Financa financa)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _financaRepositorio.Atualizar(financa);
                    TempData["MensagemSucesso"] = "Mês/ano atualizado com sucesso!";
                    return RedirectToAction("Index");
                }
                return View(financa);
            }
            catch (System.Exception erro)
            {
                TempData["MensagemErro"] = $"Ops, não foi possivel atualizar o mês/ano!, detalhe do erro:{erro.Message}";
                return RedirectToAction("Index");
            }
        }

        public IActionResult DeletarMesAno(int id)
        {
            var edit = _financaRepositorio.BuscarMesAnoPorId(id);
            return View(edit);
        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            try
            {
                bool apagado = _financaRepositorio.Apagar(id);

                if (apagado)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Ops, não foi possível excluir o mês/ano." });
                }
            }
            catch (Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível excluir o mês/ano. Detalhes do erro: {erro.Message}" });
            }
        }

    }

}



