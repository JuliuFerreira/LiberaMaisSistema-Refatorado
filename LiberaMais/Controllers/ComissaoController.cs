using LiberaMais.Filters;
using LiberaMais.Models;
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

            // 6. CÁLCULO DO SALÁRIO MENSAL (Baseado apenas nas vendas que já passaram pelo filtro acima)
            var comissoesPagasNoMes = listaRelatorio.Where(r =>
                r.Venda.DataPgtoComissao.HasValue &&
                r.Venda.DataPgtoComissao.Value.Month == mes &&
                r.Venda.DataPgtoComissao.Value.Year == ano
            ).ToList();

            decimal totalSalarioMes = comissoesPagasNoMes.Sum(r => r.Venda.ValorComissao ?? 0m);
            int quantidadePagaMes = comissoesPagasNoMes.Count;

            // 7. Alimenta as ViewBags para renderizar as opções na tela sem perder o estado
            ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos()
                .Select(u => new { Id = u.Id, Nome = u.Nome })
                .ToList();

            ViewBag.FiltroAtual = filtroStatus;
            ViewBag.UsuarioAtual = usuarioId;
            ViewBag.MesFechamento = mes;
            ViewBag.AnoFechamento = ano;
            ViewBag.TotalSalarioMes = totalSalarioMes;
            ViewBag.QuantidadePagaMes = quantidadePagaMes;

            return View(listaRelatorio);
        }
    }
}
