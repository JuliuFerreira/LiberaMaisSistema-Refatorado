using LiberaMais.Filters;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Controllers
{
      

    public class ComissaoController : Controller
    {
        private readonly IVendaRepositorio _vendaRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public ComissaoController(IVendaRepositorio vendaRepositorio, IUsuarioRepositorio usuarioRepositorio)
        {
            _vendaRepositorio = vendaRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
        }

        [PaginaRestritaSomenteAdmin]
        // Rota única que gerencia os filtros superiores e o cálculo de fechamento inferior
        public IActionResult Relatorio(string? filtroStatus, int? usuarioId, int? mesFechamento, int? anoFechamento)
        {
            // 1. Configuração do Mês e Ano do fechamento (Padrão: mês/ano atuais)
            int mes = mesFechamento ?? DateTime.Today.Month;
            int ano = anoFechamento ?? DateTime.Today.Year;

            // 2. Busca TODAS as vendas do repositório
            var vendas = _vendaRepositorio.ListarTodasVendas();

            // 3. FILTRO POR USUÁRIO (Tabela Principal)
            if (usuarioId.HasValue)
            {
                vendas = vendas.Where(v => v.UsuarioId == usuarioId.Value).ToList();
            }

            // 4. Mapeia para a ViewModel para obter a regra do StatusFinanceiro
            var listaRelatorio = vendas.Select(v => new ComissaoRelatorioViewModel
            {
                Venda = v
            }).ToList();

            // 5. FILTRO POR STATUS FINANCEIRO (Tabela Principal)
            if (!string.IsNullOrEmpty(filtroStatus))
            {
                listaRelatorio = listaRelatorio.Where(r => r.StatusFinanceiro == filtroStatus).ToList();
            }

            //// 6. CÁLCULO DO SALÁRIO MENSAL (Baseado apenas nas vendas que já passaram pelo filtro acima)
            //var comissoesPagasNoMes = listaRelatorio.Where(r =>
            //    r.Venda.DataPgtoComissao.HasValue &&
            //    r.Venda.DataPgtoComissao.Value.Month == mes &&
            //    r.Venda.DataPgtoComissao.Value.Year == ano
            //).ToList();

            //decimal totalSalarioMes = comissoesPagasNoMes.Sum(r => r.Venda.ValorComissao ?? 0m);
            //int quantidadePagaMes = comissoesPagasNoMes.Count;

            // 7. Alimenta as ViewBags para renderizar as opções na tela sem perder o estado
            ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos()
                .Select(u => new { Id = u.Id, Nome = u.Nome })
                .ToList();

            ViewBag.FiltroAtual = filtroStatus;
            ViewBag.UsuarioAtual = usuarioId;
            ViewBag.MesFechamento = mes;
            ViewBag.AnoFechamento = ano;
            //ViewBag.TotalSalarioMes = totalSalarioMes;
            //ViewBag.QuantidadePagaMes = quantidadePagaMes;

            return View(listaRelatorio);
        }

        public IActionResult RegistrarRepasse(int vendaId)
        {
            var venda = _vendaRepositorio.BuscarVendaPorId(vendaId);

            if (venda == null)
            {
                TempData["mensagemErro"] = "Venda não encontrada.";
                return RedirectToAction("Relatorio");
            }

            return View(venda);
        }

        [HttpPost]
        public IActionResult RegistrarRepasse(Venda venda)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Não foi possível registrar o repasse.";
                return View(venda);
            }

            var vendaDb = _vendaRepositorio.BuscarVendaPorId(venda.Id);

            if(vendaDb == null)
            {
                TempData["MensagemErro"] = "Venda não encontrada.";
                return RedirectToAction("Relatorio");
            }


            vendaDb.ValorRepasse = venda.ValorRepasse;
            vendaDb.DataRepasse = venda.DataRepasse;
            vendaDb.RepasseComissao = venda.RepasseComissao;
            vendaDb.ObservacaoRepasse = venda.ObservacaoRepasse;
            vendaDb.ValorContrato = venda.ValorContrato;

            _vendaRepositorio.Atualizar(vendaDb);
            TempData["MensagemSucesso"] = "Repasse de comissão registrado com sucesso.";

            return RedirectToAction("Relatorio");
        }

        public IActionResult RelatorioDePagamento()
        {
            return View(new List<ComissaoRelatorioViewModel>());
        }


        [HttpPost]
        public IActionResult RelatorioDePagamento(List<int> vendasSelecionadas)
        {
            if(vendasSelecionadas == null || !vendasSelecionadas.Any())
            {
                TempData["MensagemErro"] = "Nenhum contrato foi selecionado.";
                return RedirectToAction("Relatorio");
            }

            var vendas = _vendaRepositorio.ListarTodasVendas();

            var vendasSelecionadasBanco = vendas.Where(v => vendasSelecionadas.Contains(v.Id)).ToList();

            var listaRelatorio = vendasSelecionadasBanco.Select(v => new ComissaoRelatorioViewModel
            {
                Venda = v
            }).ToList();

            return View(listaRelatorio);
        }


        [HttpPost]
        public IActionResult EfetivarPagamento(List<int> vendasSelecionadas)
        {
            if (vendasSelecionadas == null || !vendasSelecionadas.Any())
            {
                TempData["MensagemErro"] = "Nenhuma venda foi selecionada para pagamento.";
                return RedirectToAction("Relatorio");
            }

            var vendas = _vendaRepositorio.ListarTodasVendas();

            var vendasParaPagar = vendas
                .Where(v => vendasSelecionadas.Contains(v.Id))
                .ToList();

            var vendasJaPagas = vendasParaPagar
             .Where(v => v.RepasseComissao == StatusRepasseComissaoEnum.Pago)
             .ToList();

            if (vendasJaPagas.Any())
            {
                TempData["MensagemErro"] =
                    "Uma ou mais vendas selecionadas já possuem o pagamento de comissão efetivado.";

                return RedirectToAction("Relatorio");
            }


            var valorTotal = vendasParaPagar
                .Sum(v => v.ValorRepasse ?? 0);

            var usuario = vendasParaPagar
                .First()
                .Usuario;

            foreach (var venda in vendasParaPagar)
            {
                venda.RepasseComissao = StatusRepasseComissaoEnum.Pago;
                venda.DataRepasse = DateTime.Now;
                venda.Id = venda.Id;
            }

            _vendaRepositorio.SalvarAlteracoes();

            var idVendas = string.Join(",", vendasParaPagar.Select(v => v.Id));

            TempData["FinancaData"] = DateTime.Now.ToString("yyyy-MM-dd");
            TempData["FinancaValor"] = valorTotal.ToString(System.Globalization.CultureInfo.InvariantCulture);
            TempData["FinancaTipo"] = "Despesa";
            TempData["FinancaDescricao"] = $"Pagamento referente à comissão do(s) contrato(s) ({idVendas}) do usuário {usuario?.Nome}";
            TempData["MensagemSucesso"] = "Pagamento efetivado com sucesso.";

            return RedirectToAction("Criar", "Financa");
        }


    }
}
