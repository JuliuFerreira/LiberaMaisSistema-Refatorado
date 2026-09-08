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

        public AppsController(
    IUsuarioRepositorio usuarioRepositorio,
    ISessao sessao,
    BancoContext bancoContext,
    IFinancaRepositorio financaRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _sessao = sessao;
            _bancoContext = bancoContext;
            _financaRepositorio = financaRepositorio;
        }

        public IActionResult Index(int? usuarioId, int? mes, int? ano)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            bool isAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            ViewBag.IsAdmin = isAdmin;

            // ============================================================
            // CONFIGURAÇÃO DOS FILTROS
            // ============================================================

            int mesAtual = mes ?? DateTime.Now.Month;

            int anoAtual = ano ?? DateTime.Now.Year;

            ViewBag.Mes = mesAtual;
            ViewBag.Ano = anoAtual;


            // ============================================================
            // USUÁRIO
            // ============================================================

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


            // ============================================================
            // CONSULTA DAS VENDAS
            // ============================================================

            var queryVendas = _bancoContext.Vendas
                .Include(v => v.Banco)
                .Include(v => v.Promotora)
                .AsQueryable();


            // ============================================================
            // FILTRO POR PERÍODO
            // ============================================================

            if (mesAtual > 0 && anoAtual > 0)
            {
                var dataInicio = new DateTime(anoAtual, mesAtual, 1);
                var proximoMes = dataInicio.AddMonths(1);

                queryVendas = queryVendas.Where(v =>

                    (
                        (
                            v.StatusContrato == StatusEnum.Digitado ||
                            v.StatusContrato == StatusEnum.Assinado ||
                            v.StatusContrato == StatusEnum.Cancelado
                        )
                        &&
                        v.DataCadastro >= dataInicio &&
                        v.DataCadastro < proximoMes
                    )

                    ||

                    (
                        v.StatusContrato == StatusEnum.Pago &&
                        v.DataPgtoContrato.HasValue &&
                        v.DataPgtoContrato.Value >= dataInicio &&
                        v.DataPgtoContrato.Value < proximoMes
                    )

                    ||

                    (
                        v.StatusContrato == StatusEnum.ComissaoPaga &&
                        v.DataPgtoComissao.HasValue &&
                        v.DataPgtoComissao.Value >= dataInicio &&
                        v.DataPgtoComissao.Value < proximoMes
                    )
                );
            }


            // ============================================================
            // TODOS OS MESES DE UM ANO
            // ============================================================

            else if (mesAtual == 0 && anoAtual > 0)
            {
                var dataInicio = new DateTime(anoAtual, 1, 1);
                var proximoAno = dataInicio.AddYears(1);

                queryVendas = queryVendas.Where(v =>

                    (
                        (
                            v.StatusContrato == StatusEnum.Digitado ||
                            v.StatusContrato == StatusEnum.Assinado ||
                            v.StatusContrato == StatusEnum.Cancelado
                        )
                        &&
                        v.DataCadastro >= dataInicio &&
                        v.DataCadastro < proximoAno
                    )

                    ||

                    (
                        v.StatusContrato == StatusEnum.Pago &&
                        v.DataPgtoContrato.HasValue &&
                        v.DataPgtoContrato.Value >= dataInicio &&
                        v.DataPgtoContrato.Value < proximoAno
                    )

                    ||

                    (
                        v.StatusContrato == StatusEnum.ComissaoPaga &&
                        v.DataPgtoComissao.HasValue &&
                        v.DataPgtoComissao.Value >= dataInicio &&
                        v.DataPgtoComissao.Value < proximoAno
                    )
                );
            }


            // ============================================================
            // UM MÊS EM TODOS OS ANOS
            // ============================================================

            else if (mesAtual > 0 && anoAtual == 0)
            {
                queryVendas = queryVendas.Where(v =>

                    (
                        (
                            v.StatusContrato == StatusEnum.Digitado ||
                            v.StatusContrato == StatusEnum.Assinado ||
                            v.StatusContrato == StatusEnum.Cancelado
                        )
                        &&
                        v.DataCadastro.HasValue &&
                        v.DataCadastro.Value.Month == mesAtual
                    )

                    ||

                    (
                        v.StatusContrato == StatusEnum.Pago &&
                        v.DataPgtoContrato.HasValue &&
                        v.DataPgtoContrato.Value.Month == mesAtual
                    )

                    ||

                    (
                        v.StatusContrato == StatusEnum.ComissaoPaga &&
                        v.DataPgtoComissao.HasValue &&
                        v.DataPgtoComissao.Value.Month == mesAtual
                    )
                );
            }


            // ============================================================
            // FILTRO POR USUÁRIO
            // ============================================================

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                queryVendas = queryVendas.Where(v =>
                    v.UsuarioId == usuarioId.Value);
            }


            // ============================================================
            // EXECUTA CONSULTA
            // ============================================================

            var listaContratos = queryVendas.ToList();


            // ============================================================
            // RESUMO GERAL
            // ============================================================

            var resumoGeral = new ResumoOperacaoViewModel
            {
                NomeOperacao = "Geral",

                Quantidade = listaContratos.Count,

                TotalValorContrato = listaContratos
                    .Sum(v => v.ValorContrato ?? 0m),

                TotalSaldoDevedor = listaContratos
                    .Where(v => v.Operacao == OperacaoEnum.PORTABILIDADE)
                    .Sum(v => v.SaldoDevedor ?? 0m),

                Status = listaContratos
                    .GroupBy(v => v.StatusContrato)
                    .Select(statusGroup => new ResumoStatusViewModel
                    {
                        Status = statusGroup.Key switch
                        {
                            StatusEnum.Digitado => "Aguardando Assinatura",
                            StatusEnum.Assinado => "Assinados",
                            StatusEnum.Pago => "Contratos Pagos",
                            StatusEnum.ComissaoPaga => "Comissão Paga",
                            StatusEnum.Cancelado => "Cancelados",

                            _ => statusGroup.Key.ToString()
                        },

                        Quantidade = statusGroup.Count(),

                        TotalValorContrato = statusGroup
                            .Sum(v => v.ValorContrato ?? 0m),

                        TotalSaldoDevedor = statusGroup
                            .Where(v => v.Operacao == OperacaoEnum.PORTABILIDADE)
                            .Sum(v => v.SaldoDevedor ?? 0m)

                    })
                    .OrderByDescending(x => x.Quantidade)
                    .ToList()
            };

            ViewBag.ResumoGeral = resumoGeral;


            var resumoOperacoes = listaContratos
                .GroupBy(v => v.Operacao)
                .Select(operacaoGroup => new ResumoOperacaoViewModel
                {
                    NomeOperacao = operacaoGroup.Key switch
                    {
                        OperacaoEnum.NOVO => "Novo",
                        OperacaoEnum.CartaoBeneficio => "Cartão Benefício",
                        OperacaoEnum.CartaoConsignado => "Cartão Consignado",
                        OperacaoEnum.REFINANCIAMENTO => "Refinanciamento",
                        OperacaoEnum.PORTABILIDADE => "Portabilidade",
                        OperacaoEnum.RefinDaPort => "Refin da Port",
                        OperacaoEnum.SaqueComplementar => "Saque Complementar",
                        OperacaoEnum.SaqueAniversario => "Saque Aniversário",
                        OperacaoEnum.AUMENTO => "Aumento",
                        OperacaoEnum.RefinAuto => "Refin Auto",

                        _ => operacaoGroup.Key.ToString()
                    },

                    Quantidade = operacaoGroup.Count(),

                    TotalValorContrato = operacaoGroup
                        .Sum(v => v.ValorContrato ?? 0m),

                    TotalSaldoDevedor = operacaoGroup
                        .Where(v => v.Operacao == OperacaoEnum.PORTABILIDADE)
                        .Sum(v => v.SaldoDevedor ?? 0m),

                    Status = operacaoGroup
                        .GroupBy(v => v.StatusContrato)
                        .Select(statusGroup => new ResumoStatusViewModel
                        {
                            Status = statusGroup.Key switch
                            {
                                StatusEnum.Digitado => "Aguardando Assinatura",
                                StatusEnum.Assinado => "Assinados",
                                StatusEnum.Pago => "Contratos Pagos",
                                StatusEnum.ComissaoPaga => "Comissão Paga",
                                StatusEnum.Cancelado => "Cancelados",

                                _ => statusGroup.Key.ToString()
                            },

                            Quantidade = statusGroup.Count(),

                            TotalValorContrato = statusGroup
                                .Sum(v => v.ValorContrato ?? 0m),

                            TotalSaldoDevedor = statusGroup
                                .Where(v => v.Operacao == OperacaoEnum.PORTABILIDADE)
                                .Sum(v => v.SaldoDevedor ?? 0m)

                        })
                        .OrderByDescending(x => x.Quantidade)
                        .ToList()
                })
                .OrderByDescending(x => x.Quantidade)
                .ToList();

            ViewBag.ResumoOperacoes = resumoOperacoes;


            // ============================================================
            // REGRA DE ATRASO GERAL
            // ============================================================

            var dataLimite = DateTime.Now.AddDays(-5);

            var queryAtrasosGeral = _bancoContext.Vendas
                .Where(c =>
                    c.StatusContrato == StatusEnum.Pago &&
                    c.DataPgtoContrato.HasValue &&
                    c.DataPgtoContrato.Value < dataLimite);

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                queryAtrasosGeral = queryAtrasosGeral.Where(c =>
                    c.UsuarioId == usuarioId.Value);
            }

            int totalAtrasadosGeral = queryAtrasosGeral.Count();

            bool possuiAtrasoGeral = totalAtrasadosGeral > 0;


            // ============================================================
            // VIEWMODEL
            // ============================================================

            DashboardVendaViewModel dashboard = new DashboardVendaViewModel
            {
                Digitado = listaContratos.Count(c =>
                    c.StatusContrato == StatusEnum.Digitado),

                Assinado = listaContratos.Count(c =>
                    c.StatusContrato == StatusEnum.Assinado),

                Pago = listaContratos.Count(c =>
                    c.StatusContrato == StatusEnum.Pago),

                Cancelado = listaContratos.Count(c =>
                    c.StatusContrato == StatusEnum.Cancelado),

                ComissaoPaga = listaContratos.Count(c =>
                    c.StatusContrato == StatusEnum.ComissaoPaga),

                PossuiComissaoAtrasada = possuiAtrasoGeral,

                ContratosComissaoAtrasada = totalAtrasadosGeral,

                ListaContratos = listaContratos
            };


            // ============================================================
            // CARDS OPERACIONAIS
            // ============================================================

            ViewBag.TotalDigitados =
                dashboard.Digitado +
                dashboard.Assinado +
                dashboard.Pago +
                dashboard.ComissaoPaga +
                dashboard.Cancelado;

            ViewBag.TotalAndamento =
                dashboard.Digitado +
                dashboard.Assinado;

            ViewBag.TotalConcluidos =
                dashboard.Pago +
                dashboard.ComissaoPaga;

            ViewBag.ValorDigitados =
                listaContratos
                    .Where(v => v.StatusContrato == StatusEnum.Digitado)
                    .Sum(v => v.ValorContrato ?? 0m);

            ViewBag.ValorDigitadoPort =
                listaContratos
                    .Where(v => v.StatusContrato == StatusEnum.Digitado)
                    .Sum(v => v.SaldoDevedor ?? 0m);

            ViewBag.ValorValorPagos =
                listaContratos
                    .Where(v =>
                        v.StatusContrato == StatusEnum.Pago ||
                        v.StatusContrato == StatusEnum.ComissaoPaga)
                    .Sum(v => v.ValorContrato ?? 0m);

            ViewBag.ValorPago =
                listaContratos
                    .Where(v =>
                        v.StatusContrato == StatusEnum.Pago ||
                        v.StatusContrato == StatusEnum.ComissaoPaga)
                    .Sum(c => c.ValorContrato ?? 0m);

            ViewBag.ValorPagoPort =
                listaContratos
                    .Where(v =>
                        v.StatusContrato == StatusEnum.Pago ||
                        v.StatusContrato == StatusEnum.ComissaoPaga)
                    .Sum(c => c.SaldoDevedor ?? 0m);


            // ============================================================
            // VALOR VENDIDO
            // ============================================================

            var queryValorVendido = _bancoContext.Venda
                .Where(c =>
                    c.StatusContrato == StatusEnum.Pago ||
                    c.StatusContrato == StatusEnum.ComissaoPaga);

            if (mesAtual > 0 && anoAtual > 0)
            {
                var dataInicio = new DateTime(anoAtual, mesAtual, 1);
                var proximoMes = dataInicio.AddMonths(1);

                queryValorVendido = queryValorVendido.Where(c =>
                    c.DataPgtoContrato.HasValue &&
                    c.DataPgtoContrato.Value >= dataInicio &&
                    c.DataPgtoContrato.Value < proximoMes);
            }
            else if (mesAtual == 0 && anoAtual > 0)
            {
                var dataInicio = new DateTime(anoAtual, 1, 1);
                var proximoAno = dataInicio.AddYears(1);

                queryValorVendido = queryValorVendido.Where(c =>
                    c.DataPgtoContrato.HasValue &&
                    c.DataPgtoContrato.Value >= dataInicio &&
                    c.DataPgtoContrato.Value < proximoAno);
            }
            else if (mesAtual > 0 && anoAtual == 0)
            {
                queryValorVendido = queryValorVendido.Where(c =>
                    c.DataPgtoContrato.HasValue &&
                    c.DataPgtoContrato.Value.Month == mesAtual);
            }

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                queryValorVendido = queryValorVendido.Where(c =>
                    c.UsuarioId == usuarioId.Value);
            }

            ViewBag.ValorVendido =
                queryValorVendido.Sum(c => c.ValorContrato);


            // ============================================================
            // COMISSÕES RECEBIDAS
            // ============================================================

            var queryComissaoRecebida = _bancoContext.Venda
                .Where(c =>
                    c.StatusContrato == StatusEnum.ComissaoPaga &&
                    c.DataPgtoComissao.HasValue);

            if (mesAtual > 0 && anoAtual > 0)
            {
                var dataInicio = new DateTime(anoAtual, mesAtual, 1);
                var proximoMes = dataInicio.AddMonths(1);

                queryComissaoRecebida = queryComissaoRecebida.Where(c =>
                    c.DataPgtoComissao.Value >= dataInicio &&
                    c.DataPgtoComissao.Value < proximoMes);
            }
            else if (mesAtual == 0 && anoAtual > 0)
            {
                var dataInicio = new DateTime(anoAtual, 1, 1);
                var proximoAno = dataInicio.AddYears(1);

                queryComissaoRecebida = queryComissaoRecebida.Where(c =>
                    c.DataPgtoComissao.Value >= dataInicio &&
                    c.DataPgtoComissao.Value < proximoAno);
            }
            else if (mesAtual > 0 && anoAtual == 0)
            {
                queryComissaoRecebida = queryComissaoRecebida.Where(c =>
                    c.DataPgtoComissao.Value.Month == mesAtual);
            }

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                queryComissaoRecebida = queryComissaoRecebida.Where(c =>
                    c.UsuarioId == usuarioId.Value);
            }

            ViewBag.ComissaoRecebida =
                queryComissaoRecebida.Sum(c => c.ValorComissao);


            // ============================================================
            // COMISSÕES EM ATRASO
            // ============================================================

            ViewBag.QuantidadeComissaoAtrasada =
                listaContratos
                    .Count(c => c.StatusContrato == StatusEnum.Pago);

            ViewBag.ComissaoAtraso =
                listaContratos
                    .Where(c => c.StatusContrato == StatusEnum.Pago)
                    .Sum(c => c.ValorComissao ?? 0m);


            // ============================================================
            // GRÁFICO - BANCOS
            // ============================================================

            var dadosBancos = listaContratos
                .Where(v => v.Banco != null)
                .GroupBy(v => v.Banco.Nome)
                .Select(g => new
                {
                    Banco = g.Key,
                    Qtd = g.Count()
                })
                .OrderByDescending(x => x.Qtd)
                .Take(5)
                .ToList();

            ViewBag.GraficoBancosLabels =
                dadosBancos.Select(x => x.Banco).ToList();

            ViewBag.GraficoBancosValores =
                dadosBancos.Select(x => x.Qtd).ToList();


            // ============================================================
            // GRÁFICO - PROMOTORAS
            // ============================================================

            var dadosPromotoras = listaContratos
                .Where(v => v.Promotora != null)
                .GroupBy(v => v.Promotora.Nome)
                .Select(g => new
                {
                    Promotora = g.Key,
                    Qtd = g.Count()
                })
                .OrderByDescending(x => x.Qtd)
                .Take(5)
                .ToList();

            ViewBag.GraficoPromotorasLabels =
                dadosPromotoras.Select(x => x.Promotora).ToList();

            ViewBag.GraficoPromotorasValores =
                dadosPromotoras.Select(x => x.Qtd).ToList();


            // ============================================================
            // TAXAS PARA CALCULADORA
            // ============================================================

            ViewBag.TaxasCoeficientes = _bancoContext.TaxaCoeficiente
                .Where(t => t.Ativo)
                .OrderBy(t => t.Taxa)
                .ToList();


            // ============================================================
            // RETORNO
            // ============================================================

            return View(dashboard);
        }
    }
}

