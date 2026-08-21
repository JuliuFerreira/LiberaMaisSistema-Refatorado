using LiberaMais.Data;
using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LiberaMais.Controllers
{
    [PaginaParaUsuarioLogado]
    public class AppsController : Controller
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly ISessao _sessao;
        private readonly BancoContext _bancoContext;
        private readonly IFinancaRepositorio _financaRepositorio;

        public AppsController(IUsuarioRepositorio usuarioRepositorio, ISessao sessao, BancoContext bancoContext, IFinancaRepositorio financaRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _sessao = sessao;
            _bancoContext = bancoContext;
            _financaRepositorio = financaRepositorio;
        }

        public IActionResult Index(int? usuarioId, int? mes, int? ano)
        {
            int mesAtual = mes ?? DateTime.Now.Month;
            int anoAtual = ano ?? DateTime.Now.Year;

            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            bool isAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.Mes = mesAtual;
            ViewBag.Ano = anoAtual;

            if (isAdmin)
            {
                ViewBag.UsuariosList = _usuarioRepositorio.BuscarTodos();

                if (usuarioId == null && !Request.Query.ContainsKey("usuarioId"))
                {
                    usuarioId = usuarioLogado.Id;
                }

                ViewBag.UsuarioAtual = usuarioId;
            }
            else
            {
                usuarioId = usuarioLogado.Id;
                ViewBag.UsuarioAtual = usuarioId;
            }

            // Data do mês selecionado para os Cards de Vendas e Gráficos
            var dataInicio = new DateTime(anoAtual, mesAtual, 1);
            var dataFim = dataInicio.AddMonths(1).AddDays(-1);

            // 1. Busca das Vendas filtradas pelo MÊS SELECIONADO
            var queryVendas = _bancoContext.Vendas
                .Include(v => v.Banco)
                .Include(v => v.Promotora)
                .Where(v => v.DataCadastro >= dataInicio && v.DataCadastro <= dataFim);

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                queryVendas = queryVendas.Where(v => v.UsuarioId == usuarioId.Value);
            }

            var listaContratos = queryVendas.ToList();

            // 2. REGRA DE ATRASO GERAL (INDEPENDENTE DO MÊS SELECIONADO)
            var dataLimite = DateTime.Now.AddDays(-5);

            // Consulta separada no banco pegando TODOS os contratos pendentes de comissão até hoje
            var queryAtrasosGeral = _bancoContext.Vendas
                .Where(c => c.StatusContrato == StatusEnum.Pago &&
                            c.DataPgtoContrato.HasValue &&
                            c.DataPgtoContrato.Value < dataLimite);

            // Mantém a regra de segurança/filtro do usuário também no alerta
            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                queryAtrasosGeral = queryAtrasosGeral.Where(c => c.UsuarioId == usuarioId.Value);
            }

            int totalAtrasadosGeral = queryAtrasosGeral.Count();
            bool possuiAtrasoGeral = totalAtrasadosGeral > 0;

            // 3. MONTAGEM DA VIEWMODEL
            DashboardVendaViewModel dashboard = new DashboardVendaViewModel
            {
                Digitado = listaContratos.Count(c => c.StatusContrato == StatusEnum.Digitado),
                Assinado = listaContratos.Count(c => c.StatusContrato == StatusEnum.Assinado),
                Pago = listaContratos.Count(c => c.StatusContrato == StatusEnum.Pago),
                Cancelado = listaContratos.Count(c => c.StatusContrato == StatusEnum.Cancelado),
                ComissaoPaga = listaContratos.Count(c => c.StatusContrato == StatusEnum.ComissaoPaga),

                // Usamos as variáveis da consulta GERAL de atrasos
                PossuiComissaoAtrasada = possuiAtrasoGeral,
                ContratosComissaoAtrasada = totalAtrasadosGeral,

                ListaContratos = listaContratos
            };

            // 4. DADOS DOS CARDS DO MÊS
            ViewBag.TotalDigitados = dashboard.Digitado + dashboard.Assinado + dashboard.Pago + dashboard.ComissaoPaga + dashboard.Cancelado;
            ViewBag.TotalAndamento = dashboard.Digitado + dashboard.Assinado;
            ViewBag.TotalConcluidos = dashboard.Pago + dashboard.ComissaoPaga;

            ViewBag.ValorVendido = _bancoContext.Venda.Where(c => c.StatusContrato == StatusEnum.Pago || c.StatusContrato == StatusEnum.ComissaoPaga).Where(c=> c.DataPgtoContrato.HasValue && c.DataPgtoContrato.Value >= dataInicio && c.DataPgtoContrato.Value <= dataFim).Where(c=> !usuarioId.HasValue || usuarioId.Value <= 0 || c.UsuarioId == usuarioId.Value).Sum(c => c.ValorContrato);
            ViewBag.ComissaoRecebida = _bancoContext.Venda.Where(c => c.StatusContrato == StatusEnum.ComissaoPaga).Where(c => c.DataPgtoComissao.HasValue && c.DataPgtoComissao.Value >= dataInicio && c.DataPgtoComissao.Value <= dataFim).Where(c => !usuarioId.HasValue || usuarioId.Value <= 0 || c.UsuarioId == usuarioId.Value).Sum(c => c.ValorComissao);
            ViewBag.ComissaoAtraso = listaContratos.Where(c => c.StatusContrato == StatusEnum.Pago).Sum(c => c.ValorComissao);

            // 5. GRÁFICOS
            var dadosBancos = listaContratos
                .Where(v => v.Banco != null)
                .GroupBy(v => v.Banco.Nome)
                .Select(g => new { Banco = g.Key, Qtd = g.Count() })
                .OrderByDescending(x => x.Qtd)
                .Take(5)
                .ToList();

            ViewBag.GraficoBancosLabels = dadosBancos.Select(x => x.Banco).ToList();
            ViewBag.GraficoBancosValores = dadosBancos.Select(x => x.Qtd).ToList();

            var dadosPromotoras = listaContratos
                .Where(v => v.Promotora != null)
                .GroupBy(v => v.Promotora.Nome)
                .Select(g => new { Promotora = g.Key, Qtd = g.Count() })
                .OrderByDescending(x => x.Qtd)
                .Take(5)
                .ToList();

            ViewBag.GraficoPromotorasLabels = dadosPromotoras.Select(x => x.Promotora).ToList();
            ViewBag.GraficoPromotorasValores = dadosPromotoras.Select(x => x.Qtd).ToList();

            return View(dashboard);
        }
    }
}
