using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using LiberaMais.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;

namespace LiberaMais.Controllers
{
    [PaginaParaUsuarioLogado]
    public class VendaController : Controller
    {
        private readonly IVendaRepositorio _vendaRepositorio;
        private readonly ISessao _sessao;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IClienteRepositorio _clienteRepositorio;
        private readonly IClienteBeneficioRepositorio _clienteBeneficioRepositorio;
        private readonly IPromotorasRepositorio _promotorasRepositorio;
        private readonly IBancosRepositorio _bancosRepositorio;
        private readonly IBeneficioRepositorio _beneficioRepositorio;
        private readonly IPromotoraBancoRepositorio _promotoraBancoRepositorio;
        private readonly PermissaoService _permissaoService;

        public VendaController(IVendaRepositorio vendaRepositorio,
            ISessao sessao, IUsuarioRepositorio usuarioRepositorio,
            IClienteRepositorio clienteRepositorio,
            IClienteBeneficioRepositorio clienteBeneficioRepositorio,
            IPromotorasRepositorio promotorasRepositorio,
            IBancosRepositorio bancosRepositorio,
            IPromotoraBancoRepositorio promotoraBancoRepositorio,
            IBeneficioRepositorio beneficioRepositorio,
            PermissaoService permissaoService
            )
        {
            _vendaRepositorio = vendaRepositorio;
            _sessao = sessao;
            _usuarioRepositorio = usuarioRepositorio;
            _clienteRepositorio = clienteRepositorio;
            _clienteBeneficioRepositorio = clienteBeneficioRepositorio;
            _promotorasRepositorio = promotorasRepositorio;
            _bancosRepositorio = bancosRepositorio;
            _promotoraBancoRepositorio = promotoraBancoRepositorio;
            _beneficioRepositorio = beneficioRepositorio;
            _permissaoService = permissaoService;
        }

        public void CarregarCombos()
        {
            ViewBag.Usuario = _usuarioRepositorio.ListarTodosUsuarios();
            ViewBag.Cliente = _clienteRepositorio.ListarTodosClientes();
            ViewBag.ClienteBeneficio = _clienteBeneficioRepositorio.ListarTodos();
            ViewBag.Promotora = _promotorasRepositorio.ListarPromotora();
            ViewBag.Banco = _bancosRepositorio.ListarBancos();
            ViewBag.Beneficio = _beneficioRepositorio.ListarTodos();
        }

        private bool ComissaoEstaAtrasada(Venda venda)
        {
            if (venda.StatusContrato != StatusEnum.Pago ||
                !venda.DataPgtoContrato.HasValue)
            {
                return false;
            }

            var dataPagamento = venda.DataPgtoContrato.Value;

            // =========================================================
            // BANCO 34 - BRB (RED CONSIG)
            // =========================================================
            if (venda.BancoId == 34)
            {
                DateTime dataPrevistaComissao;

                if (dataPagamento.Day <= 15)
                {
                    // Pagamento de 01 a 15
                    // Comissão no último dia do mesmo mês

                    dataPrevistaComissao = new DateTime(
                        dataPagamento.Year,
                        dataPagamento.Month,
                        DateTime.DaysInMonth(
                            dataPagamento.Year,
                            dataPagamento.Month));
                }
                else
                {
                    // Pagamento de 16 ao final do mês
                    // Comissão no dia 15 do mês seguinte

                    var proximoMes = dataPagamento.AddMonths(1);

                    dataPrevistaComissao = new DateTime(
                        proximoMes.Year,
                        proximoMes.Month,
                        15);
                }

                return DateTime.Now.Date > dataPrevistaComissao.Date;
            }

            // =========================================================
            // DEMAIS BANCOS
            // Regra atual: 5 dias após o pagamento
            // =========================================================

            var dataLimite = DateTime.Now.AddDays(-5);

            return dataPagamento < dataLimite;
        }

        public JsonResult BuscarBancosPorPromotora(int promotoraId)
        {
            var bancos = _promotoraBancoRepositorio.ListarPorPromotora(promotoraId)
                .Select(p => new
                {
                    id = p.Banco.Id,
                    nome = p.Banco.Nome
                })
                .ToList();
            return Json(bancos);
        }


        public IActionResult Index(int? usuarioId, int? mes, int? ano, bool todos = false)
        {
            int mesAtual = mes ?? DateTime.Now.Month;
            int anoAtual = ano ?? DateTime.Now.Year;

            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            bool isAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            ViewBag.IsAdmin = isAdmin;
            ViewBag.UsuarioAtual = isAdmin ? usuarioId : usuarioLogado.Id;

            ViewBag.Mes = mesAtual;
            ViewBag.Ano = anoAtual;
            ViewBag.Todos = todos;

            if (isAdmin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos();

                if (usuarioId == null && !Request.Query.ContainsKey("usuarioId"))
                {
                    usuarioId = usuarioLogado.Id;
                }

                ViewBag.UsuarioAtual = usuarioId;
            }

            var dataInicio = new DateTime(anoAtual, mesAtual, 1);
            var dataFim = dataInicio.AddMonths(1);

            // =========================================================
            // TODOS OS CONTRATOS
            // =========================================================

            var todosContratos = _vendaRepositorio.ListarTodasVendas();

            // =========================================================
            // VERIFICA COMISSÕES EM ATRASO
            // INDEPENDENTE DO MÊS SELECIONADO
            // =========================================================

            var contratosParaVerificarAtraso = todosContratos.AsEnumerable();

            if (isAdmin)
            {
                if (usuarioId.HasValue)
                {
                    contratosParaVerificarAtraso = contratosParaVerificarAtraso
                        .Where(c => c.UsuarioId == usuarioId.Value);
                }
            }
            else
            {
                contratosParaVerificarAtraso = contratosParaVerificarAtraso
                    .Where(c => c.UsuarioId == usuarioLogado.Id);
            }

            var contratosComissaoAtrasada = contratosParaVerificarAtraso
                .Where(c => ComissaoEstaAtrasada(c))
                .ToList();

            // =========================================================
            // FILTRO NORMAL DO MÊS
            // =========================================================

            var listaContratos = todosContratos;

            if (!todos)
            {
                listaContratos = listaContratos
                    .Where(c =>
                        (
                            (c.StatusContrato == StatusEnum.Digitado ||
                             c.StatusContrato == StatusEnum.Assinado)
                            &&
                            c.DataCadastro.HasValue &&
                            c.DataCadastro.Value < dataFim
                        )
                        ||
                        (
                            c.StatusContrato == StatusEnum.Pago &&
                            c.DataPgtoContrato.HasValue &&
                            c.DataPgtoContrato.Value >= dataInicio &&
                            c.DataPgtoContrato.Value < dataFim
                        )
                        ||
                        (
                            c.StatusContrato == StatusEnum.ComissaoPaga &&
                            c.DataPgtoComissao.HasValue &&
                            c.DataPgtoComissao.Value >= dataInicio &&
                            c.DataPgtoComissao.Value < dataFim
                        )
                    )
                    .ToList();
            }

            // =========================================================
            // FILTRO DE USUÁRIO
            // =========================================================

            if (isAdmin)
            {
                if (usuarioId.HasValue)
                {
                    listaContratos = listaContratos
                        .Where(c => c.UsuarioId == usuarioId.Value)
                        .ToList();
                }
            }
            else
            {
                listaContratos = listaContratos
                    .Where(c => c.UsuarioId == usuarioLogado.Id)
                    .ToList();
            }

            // =========================================================
            // DASHBOARD
            // =========================================================

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

                PossuiComissaoAtrasada = contratosComissaoAtrasada.Any(),

                ContratosComissaoAtrasada = contratosComissaoAtrasada.Count,

                ListaContratos = listaContratos
            };

            return View(dashboard);
        }

        public IActionResult PorStatus(string status, string busca, int? usuarioId, OperacaoEnum? operacao, int? mes, int? ano, int pagina = 1, bool atrasadas = false)
        {
            ViewBag.Banco = _bancosRepositorio.ListarBancos();
            ViewBag.OperacaoEnum = operacao;

if (usuarioId == 0)
            {
                usuarioId = null;
            }

            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            bool isAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            ViewBag.IsAdmin = isAdmin;
            ViewBag.BuscaAtual = busca;
            ViewBag.StatusParametro = status;
            ViewBag.Atrasadas = atrasadas;

            // =========================================================
            // MÊS / ANO
            // =========================================================

            int anoSelecionado = ano ?? DateTime.Now.Year;

            // Se o mês NÃO foi informado na URL, significa que o usuário
            // acabou de entrar na tela. Nesse caso, selecionamos o mês atual.
            //
            // Se "mes" foi informado e veio vazio/nulo, significa que o
            // usuário escolheu "Todos os meses".
            int? mesSelecionado = mes;

            if (!Request.Query.ContainsKey("mes") && !mes.HasValue)
            {
                mesSelecionado = DateTime.Now.Month;
            }

            DateTime? dataInicio = null;
            DateTime? dataFim = null;

            if (mesSelecionado.HasValue)
            {
                dataInicio = new DateTime(
                    anoSelecionado,
                    mesSelecionado.Value,
                    1);

                dataFim = dataInicio.Value.AddMonths(1);
            }

            ViewBag.Mes = mesSelecionado;
            ViewBag.Ano = anoSelecionado;

            // =========================================================
            // USUÁRIOS
            // =========================================================

            if (isAdmin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos();

                if (usuarioId == null &&
                    !Request.Query.ContainsKey("usuarioId") &&
                    string.IsNullOrWhiteSpace(busca))
                {
                    usuarioId = usuarioLogado.Id;
                }

                ViewBag.UsuarioAtual = usuarioId;
            }

            // =========================================================
            // PAGINAÇÃO
            // =========================================================

            int tamanhoCorte = 10;
            int totalRegistros = 0;

            List<Venda> vendas;

            if (!string.IsNullOrWhiteSpace(busca))
            {
                vendas = _vendaRepositorio.BuscarCompleto(
                    busca,
                    pagina,
                    tamanhoCorte,
                    out totalRegistros);
            }
            else
            {
                vendas = _vendaRepositorio.ListarTodasVendas();
            }

            // =========================================================
            // FILTRO DE USUÁRIO
            // =========================================================

            if (isAdmin)
            {
                if (usuarioId.HasValue)
                {
                    vendas = vendas
                        .Where(v => v.UsuarioId == usuarioId.Value)
                        .ToList();
                }
            }
            else
            {
                vendas = vendas
                    .Where(v => v.UsuarioId == usuarioLogado.Id)
                    .ToList();
            }

            // =========================================================
            // FILTRO DE OPERAÇÃO
            // =========================================================

            if (operacao.HasValue)
            {
                vendas = vendas
                    .Where(v => v.Operacao == operacao.Value)
                    .ToList();
            }

            // =========================================================
            // STATUS
            // =========================================================

            if (Enum.TryParse(typeof(StatusEnum), status, out var statusEnum))
            {
                var field = statusEnum.GetType()
                    .GetField(statusEnum.ToString());

                var attribute =
                    (System.ComponentModel.DataAnnotations.DisplayAttribute)
                    Attribute.GetCustomAttribute(
                        field,
                        typeof(System.ComponentModel.DataAnnotations.DisplayAttribute));

                ViewBag.StatusAtual = attribute != null
                    ? attribute.Name
                    : status;

                var statusSelecionado = (StatusEnum)statusEnum;

                vendas = vendas
                    .Where(v => v.StatusContrato == statusSelecionado)
                    .ToList();

                // =====================================================
                // COMISSÕES EM ATRASO
                // =====================================================

                if (statusSelecionado == StatusEnum.Pago && atrasadas)
                {
                    vendas = vendas
                        .Where(v => ComissaoEstaAtrasada(v))
                        .ToList();
                }

                // =====================================================
                // DIGITADO / ASSINADO
                // =====================================================

                else if (statusSelecionado == StatusEnum.Digitado ||
                         statusSelecionado == StatusEnum.Assinado)
                {
                    if (dataFim.HasValue)
                    {
                        vendas = vendas
                            .Where(v =>
                                v.DataCadastro.HasValue &&
                                v.DataCadastro.Value < dataFim.Value)
                            .ToList();
                    }
                }

                // =====================================================
                // PAGO - FILTRO NORMAL POR MÊS
                // =====================================================

                else if (statusSelecionado == StatusEnum.Pago)
                {
                    // Se mesSelecionado for null = Todos os meses.
                    // Nesse caso, não aplicamos filtro de mês.
                    if (dataInicio.HasValue && dataFim.HasValue)
                    {
                        vendas = vendas
                            .Where(v =>
                                v.DataPgtoContrato.HasValue &&
                                v.DataPgtoContrato.Value >= dataInicio.Value &&
                                v.DataPgtoContrato.Value < dataFim.Value)
                            .ToList();
                    }
                }

                // =====================================================
                // COMISSÃO PAGA
                // =====================================================

                else if (statusSelecionado == StatusEnum.ComissaoPaga)
                {
                    // Se mesSelecionado for null = Todos os meses.
                    if (dataInicio.HasValue && dataFim.HasValue)
                    {
                        vendas = vendas
                            .Where(v =>
                                v.DataPgtoComissao.HasValue &&
                                v.DataPgtoComissao.Value >= dataInicio.Value &&
                                v.DataPgtoComissao.Value < dataFim.Value)
                            .ToList();
                    }
                }

                // =====================================================
                // TOTAIS
                // =====================================================

                ViewBag.ValorContrato =
                    vendas.Sum(v => v.ValorContrato ?? 0);

                ViewBag.ValorSaldo =
                    vendas.Sum(v => v.SaldoDevedor ?? 0);

                ViewBag.ValorComissao =
                    vendas.Sum(v => v.ValorComissao ?? 0);

                // =====================================================
                // PAGINAÇÃO
                // =====================================================

                if (string.IsNullOrWhiteSpace(busca))
                {
                    totalRegistros = vendas.Count;

                    vendas = vendas
                        .OrderByDescending(v => v.DataCadastro)
                        .Skip((pagina - 1) * tamanhoCorte)
                        .Take(tamanhoCorte)
                        .ToList();
                }
                else
                {
                    totalRegistros = vendas.Count;
                }

                ViewBag.PaginaAtual = pagina;

                ViewBag.TotalPaginas =
                    (int)Math.Ceiling(
                        (double)totalRegistros / tamanhoCorte);

                ViewBag.TotalRegistros = totalRegistros;

                ViewBag.ContratosComissaoAtrasadaIds = vendas
                    .Where(v => ComissaoEstaAtrasada(v))
                    .Select(v => v.Id)
                    .ToHashSet();

                return View(vendas);
            }
            else
            {
                TempData["mensagemErro"] =
                    "Status inválido ou não encontrado.";

                return RedirectToAction("Index");
            }

}



        [HttpGet]
        public IActionResult BuscarClientesAjax(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return Json(new List<object>());

            // 1. Buscamos as vendas usando o seu repositório atual para achar o cliente por nome ou CPF
            int totalRegistros = 0;
            var clientesEncontrados = _clienteRepositorio.ListarTodosClientes()
                .Where(c => c.Nome.ToUpper().Contains(termo.ToUpper()) || c.Cpf.Contains(termo))
                .ToList();

            // 3. Montamos o JSON buscando os benefícios exatamente como sua rota antiga fazia
            var resultadoJson = clientesEncontrados.Select(cliente =>
            {

                // Chamada ao seu repositório original de benefícios!
                var listaBeneficios = _clienteBeneficioRepositorio.ListarBeneficiosPorCliente(cliente.Id) ?? new List<ClienteBeneficio>();



                return new
                {
                    id = cliente.Id,
                    nome = cliente.Nome,
                    cpf = cliente.Cpf,
                    beneficios = listaBeneficios.Select(cb => new
                    {
                        id = cb.Id,
                        // Garante que pega o nome do órgão e descrição mapeados corretamente do seu objeto
                        orgaoNome = cb.Beneficio?.Orgaos?.Nome ?? "N/A",
                        descricao = cb.Beneficio?.Descricao ?? "Sem descrição"
                    }).ToList()
                };


            }).ToList();

            return Json(resultadoJson);
        }

    
        [HttpGet]
        public IActionResult Criar(int? clienteId, int? usuarioId, string dataCadastro, decimal? valorParcela, bool? modoRefinPort, int? promotoraId, int? bancoId, int? beneficioId)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            ViewBag.UsuarioLogadoId = usuarioLogado.Id;

            CarregarCombos();

            var novaVenda = new Venda();

            // ==========================================
            // 1. VINCULAÇÃO DO CLIENTE E BENEFÍCIO
            // ==========================================
            if (clienteId.HasValue && clienteId.Value > 0)
            {
                var cliente = _clienteRepositorio.BuscarClientePorId(clienteId.Value);
                if (cliente != null)
                {
                    ViewBag.ClienteId = cliente.Id;
                    ViewBag.Cpf = cliente.Cpf;
                    ViewBag.ClienteNome = cliente.Nome;
                    ViewBag.Beneficios = cliente.ClienteBeneficios;

                    // Determina qual é o ID do benefício anterior
                    int idBeneficioSelecionado = 0;
                    if (beneficioId.HasValue && beneficioId.Value > 0)
                    {
                        idBeneficioSelecionado = beneficioId.Value;
                    }
                    else if (cliente.ClienteBeneficios != null && cliente.ClienteBeneficios.Count == 1)
                    {
                        idBeneficioSelecionado = cliente.ClienteBeneficios[0].Id;
                    }

                    // Injeta o ID no Model para o ASP.NET reconhecer no formulário
                    if (idBeneficioSelecionado > 0)
                    {
                        novaVenda.ClienteBeneficioId = idBeneficioSelecionado;
                    }

                    // Monta o SelectList com o item correto selecionado
                    ViewBag.BeneficiosSelectList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                        cliente.ClienteBeneficios,
                        "Id",
                        "DescricaoCompleta", // Certifique-se que essa propriedade existe no seu Model (ex: Descricao ou Numero)
                        idBeneficioSelecionado
                    );
                }
            }

            // ==========================================
            // 2. VINCULAÇÃO DE USUÁRIO E OPERAÇÃO
            // ==========================================
            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                novaVenda.UsuarioId = usuarioId.Value;
            }

            ViewBag.ModoRefinPort = modoRefinPort ?? false;

            // ==========================================
            // 3. RECUPERAÇÃO DA PARCELA ANTERIOR
            // ==========================================
            if (valorParcela.HasValue)
            {
                novaVenda.ValorParcela = valorParcela.Value; // Preenche o campo da tela Normal
                ViewBag.ValorParcelaAnterior = valorParcela;
            }

            // ==========================================
            // 4. RECUPERAÇÃO DE PROMOTORA E BANCO
            // ==========================================
            if (promotoraId.HasValue && promotoraId.Value > 0)
            {
                novaVenda.PromotoraId = promotoraId.Value;
            }
            if (bancoId.HasValue && bancoId.Value > 0)
            {
                novaVenda.BancoId = bancoId.Value;
            }

            // ==========================================
            // 5. RECUPERAÇÃO DA DATA (Formatada para HTML5)
            // ==========================================
            if (!string.IsNullOrEmpty(dataCadastro) && DateTime.TryParse(dataCadastro, out DateTime dataConvertida))
            {
                novaVenda.DataCadastro = dataConvertida;
                ViewBag.DataCadastroFormatada = dataConvertida.ToString("yyyy-MM-dd");
            }
            else
            {
                novaVenda.DataCadastro = DateTime.Now;
                ViewBag.DataCadastroFormatada = DateTime.Now.ToString("yyyy-MM-dd");
            }

            return View(novaVenda);
        }

        [HttpPost]
        public IActionResult Criar(Venda venda, string botaoSalvar)
        {
            var statusDaVenda = venda.StatusContrato;

            if (!ModelState.IsValid) { return View(venda); }

            try
            {
                if (venda.ValorComissao.HasValue && venda.ValorComissao.Value > 0)
                {
                    venda.StatusContrato = StatusEnum.ComissaoPaga;
                }

                _vendaRepositorio.Adicionar(venda);

                TempData["MensagemSucesso"] = "Venda cadastrada com sucesso!";

                if (botaoSalvar == "refin")
                {
                    // O input type="date" envia no formato correto, capturamos a string limpa
                    string dataFormatada = venda.DataCadastro?.ToString("yyyy-MM-dd");

                    // Coleta a parcela da portabilidade antiga para mandar para o Refin
                    decimal? parcelaParaPassar = venda.ValorParcelaPort.HasValue && venda.ValorParcelaPort > 0
                        ? venda.ValorParcelaPort
                        : venda.ValorParcela;

                    // Dentro do seu if (botaoSalvar == "refin") no Controller POST:
                    return RedirectToAction("Criar", new
                    {
                        clienteId = venda.ClienteId,
                        usuarioId = venda.UsuarioId,
                        dataCadastro = dataFormatada,
                        valorParcela = parcelaParaPassar,
                        modoRefinPort = true,
                        promotoraId = venda.PromotoraId,
                        bancoId = venda.BancoId,
                        beneficioId = venda.ClienteBeneficioId // <-- PASSA O BENEFÍCIO SELECIONADO ANTERIORMENTE
                    });
                }

                if (botaoSalvar == "continuar")
                {
                    string dataFormatada = venda.DataCadastro?.ToString("yyyy-MM-dd");

                    return RedirectToAction("Criar", new
                    {
                        clienteId = venda.ClienteId,
                        usuarioId = venda.UsuarioId,
                        dataCadastro = dataFormatada
                    });
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View(venda);
            }
        }
        public IActionResult Detalhe(int id)
        {
            var venda = _vendaRepositorio.BuscarVendaPorId(id);

            if (venda == null)
                return NotFound();

            return View(venda);
        }
        public IActionResult Editar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            var venda = _vendaRepositorio.BuscarVendaPorId(id);

            if (venda == null)
                return RedirectToAction("Index");

            if (!_permissaoService.UsuarioTemAcessoVenda(usuarioLogado, venda))
            {
                TempData["MensagemErro"] = "Você não tem acesso a esta venda.";
                return RedirectToAction("Index");
            }

            // Carrega os dados necessários para os Dropdowns da tela
            CarregarCombos();
            ViewBag.Beneficios = _clienteBeneficioRepositorio
                .ListarTodos()
                .Where(x => x.ClienteId == venda.ClienteId)
                .ToList();

            return View(venda);
        }

        [HttpPost]
        public IActionResult Editar(Venda venda)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            var statusDaVenda = venda.StatusContrato;


            var vendaDb = _vendaRepositorio.BuscarVendaPorId(venda.Id);


            if (vendaDb == null)
            {
                TempData["MensagemErro"] = "Venda não localizada.";
                return RedirectToAction("PorStatus", new { status = statusDaVenda });
            }

            if (!_permissaoService.UsuarioTemAcessoVenda(usuarioLogado, vendaDb))
            {
                TempData["MensagemErro"] = "Você não tem acesso a esta venda.";
                return RedirectToAction("PorStatus", new { status = statusDaVenda });
            }

            // --- REGRA DE NEGÓCIO DA COMISSÃO ---
            // Se o valor da comissão foi preenchido e é maior que zero, força o status para ComissaoPaga
            //if (venda.ValorComissao.HasValue && venda.ValorComissao.Value > 0)
            //{
            //    venda.StatusContrato = StatusEnum.ComissaoPaga; // Altere para StatusContrato se for o nome no seu Model
            //}

            if (venda.StatusContrato == StatusEnum.ComissaoPaga)
            {
                // Se a data NÃO foi informada
                if (!venda.DataPgtoComissao.HasValue)
                {
                    ModelState.AddModelError("DataPgtoComissao", "Informe a data de pagamento da comissão.");
                }

                // Se o valor NÃO foi informado OU for menor/igual a zero
                if (!venda.ValorComissao.HasValue)
                {
                    ModelState.AddModelError("ValorComissao", "Informe o valor da comissão recebida.");
                }
            }

            if (venda.StatusContrato == StatusEnum.Pago || venda.StatusContrato == StatusEnum.ComissaoPaga)
            {
                if (!venda.DataPgtoContrato.HasValue)
                {

                    ModelState.AddModelError("DataPgtoContrato", "Informe a data de pagamento do contrato.");

                }
            }


            // Ignora objetos de navegação complexos para evitar validações falsas-negativas
            ModelState.Remove("Cliente");
            ModelState.Remove("Banco");
            ModelState.Remove("Promotora");
            ModelState.Remove("ClienteBeneficio");
            ModelState.Remove("Usuario");


            if (!ModelState.IsValid)
            {
                // CRÍTICO: Recarrega os combos para a tela não quebrar ao retornar com erro de validação
                CarregarCombos();
                ViewBag.Beneficios = _clienteBeneficioRepositorio
                    .ListarTodos()
                    .Where(x => x.ClienteId == venda.ClienteId)
                    .ToList();

                // FIX: Atribui o cliente buscado do banco para o objeto que vai voltar para a View
                venda.Cliente = vendaDb?.Cliente ?? _clienteRepositorio.BuscarClientePorId(venda.ClienteId);

                TempData["MensagemErro"] = "Existem campos inválidos no formulário.";
                return View(venda);
            }

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                vendaDb.UsuarioId = venda.UsuarioId;
            }
            // Mapeamento seguro dos dados enviados pelo formulário para o objeto rastreado do Banco
            vendaDb.DataCadastro = venda.DataCadastro;
            vendaDb.ClienteBeneficioId = venda.ClienteBeneficioId;
            vendaDb.PromotoraId = venda.PromotoraId;
            vendaDb.BancoId = venda.BancoId;
            vendaDb.Operacao = venda.Operacao;
            vendaDb.ValorParcela = venda.ValorParcela;
            vendaDb.ValorContrato = venda.ValorContrato;
            vendaDb.NumeroDeParcelas = venda.NumeroDeParcelas;
            vendaDb.StatusContrato = venda.StatusContrato; // Altere para StatusContrato se necessário
            vendaDb.Observacao = venda.Observacao;
            vendaDb.BancoComprado = venda.BancoComprado;
            vendaDb.NumeroContrato = venda.NumeroContrato;
            vendaDb.ValorParcelaPort = venda.ValorParcelaPort;
            vendaDb.ParcelasRestantes = venda.ParcelasRestantes;
            vendaDb.SaldoDevedor = venda.SaldoDevedor;
            vendaDb.ValorComissao = venda.ValorComissao;
            vendaDb.DataPgtoComissao = venda.DataPgtoComissao;
            vendaDb.DataPgtoContrato = venda.DataPgtoContrato;
            try
            {

                _vendaRepositorio.Atualizar(vendaDb);
                TempData["MensagemSucesso"] = "Venda atualizada com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Recarrega os combos aqui também por segurança caso o banco dê erro de gravação
                CarregarCombos();
                ViewBag.Beneficios = _clienteBeneficioRepositorio
                    .ListarTodos()
                    .Where(x => x.ClienteId == venda.ClienteId)
                    .ToList();

                TempData["MensagemErro"] = $"Erro ao salvar: {ex.Message}";
                return View(venda);
            }
        }
        public IActionResult Deletar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            var venda = _vendaRepositorio.BuscarVendaPorId(id);

            if (venda == null)
            {
                TempData["MensagemErro"] = "Venda não localizada.";
                return RedirectToAction("PorStatus");
            }

            if (!_permissaoService.UsuarioTemAcessoVenda(usuarioLogado, venda))
            {
                TempData["MensagemErro"] = "Você não tem acesso";
                return RedirectToAction("PorStatus");
            }

            return View(venda);
        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            var venda = _vendaRepositorio.BuscarVendaPorId(id);

            if (venda == null)
            {
                TempData["MensagemErro"] = "Venda não localizada.";
                TempData.Keep("MensagemErro");
                return RedirectToAction("PorStatus");
            }

            if (!_permissaoService.UsuarioTemAcessoVenda(usuarioLogado, venda))
            {
                TempData["MensagemErro"] = "Você não tem acesso";
                TempData.Keep("MensagemErro");
                return RedirectToAction("PorStatus");
            }

            // Guardamos o status da venda antes de excluí-la para saber para onde voltar
            var statusDaVenda = venda.StatusContrato;

            try
            {
                // CORREÇÃO AQUI: Passando o status de volta na URL para a Action PorStatus não dar erro
                if (statusDaVenda == StatusEnum.Pago)
                {
                    TempData["MensagemErro"] = "Esse contrato já foi pago, não é possível excluir.";
                    TempData.Keep("MensagemErro"); // Garante que a mensagem chegue na View
                    return RedirectToAction("PorStatus", new { status = statusDaVenda });
                }

                if (statusDaVenda == StatusEnum.ComissaoPaga)
                {
                    TempData["MensagemErro"] = "Esse contrato já foi finalizado, NÃO PODE SER EXCLUÍDO.";
                    TempData.Keep("MensagemErro"); // Garante que a mensagem chegue na View
                    return RedirectToAction("PorStatus", new { status = statusDaVenda });
                }

                bool apagado = _vendaRepositorio.Apagar(id);

                if (apagado)
                {
                    TempData["MensagemSucesso"] = "Venda apagada com sucesso.";
                    return RedirectToAction("PorStatus", new { status = statusDaVenda });
                }
                else
                {
                    TempData["MensagemErro"] = "Não é possível apagar essa venda.";
                    TempData.Keep("MensagemErro");
                    return RedirectToAction("PorStatus", new { status = statusDaVenda });
                }
            }
            catch (Exception)
            {
                TempData["MensagemErro"] = "Não foi possível excluir a venda!";
                TempData.Keep("MensagemErro");
                return RedirectToAction("PorStatus", new { status = statusDaVenda });
            }
        }
    }


}

