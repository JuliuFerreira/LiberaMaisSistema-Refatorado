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

            // 1. Busca o usuário logado para sabermos se ele é Admin ou operador comum
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            bool isAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.UsuarioAtual = isAdmin ? usuarioId : usuarioLogado.Id;

            ViewBag.Mes = mesAtual;
            ViewBag.Ano = anoAtual;
            ViewBag.Todos = todos;


            // 2. Configura a lista de usuários para o dropdown no dashboard (apenas se for Admin)
            if (isAdmin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos();

                // Comportamento padrão inicial: se não escolheu um operador no dropdown ainda,
                // carrega por padrão apenas os dados do próprio Admin
                if (usuarioId == null && !Request.Query.ContainsKey("usuarioId"))
                {
                    usuarioId = usuarioLogado.Id;
                }

                ViewBag.UsuarioAtual = usuarioId;
            }

            var dataInicio = new DateTime(anoAtual, mesAtual, 1);
            var dataFim = dataInicio.AddMonths(1);

            // 3. CHAMADA DO REPOSITÓRIO: Buscamos todos os contratos do banco
            var listaContratos = _vendaRepositorio.ListarTodasVendas();

            if (!todos)
            {
                listaContratos = listaContratos
                .Where(c => c.DataCadastro >= dataInicio &&
                       c.DataCadastro < dataFim)
                .ToList();

            }

            // 4. SEGURANÇA E FILTRAGEM INTELIGENTE:
            if (isAdmin)
            {
                // Se o Admin escolheu filtrar por um usuário específico no dropdown
                if (usuarioId.HasValue)
                {
                    listaContratos = listaContratos.Where(c => c.UsuarioId == usuarioId.Value).ToList();
                }
                // Se escolheu "Todos os Usuários" (usuarioId é nulo mas o filtro foi enviado), não aplica Where
            }
            else
            {
                // Se for operador comum, trava rigidamente a segurança para ver só o dele
                listaContratos = listaContratos.Where(c => c.UsuarioId == usuarioLogado.Id).ToList();
            }

            // 5. REGRA DE NEGÓCIO: Calcula a data limite para comissão atrasada (Hoje menos 5 dias)
            var dataLimite = DateTime.Now.AddDays(-5);

            // 6. Montamos o seu Dashboard contando os status dinamicamente da lista filtrada
            DashboardVendaViewModel dashboard = new DashboardVendaViewModel
            {
                Digitado = listaContratos.Count(c => c.StatusContrato == StatusEnum.Digitado),
                Assinado = listaContratos.Count(c => c.StatusContrato == StatusEnum.Assinado),
                Pago = listaContratos.Count(c => c.StatusContrato == StatusEnum.Pago),
                Cancelado = listaContratos.Count(c => c.StatusContrato == StatusEnum.Cancelado),
                ComissaoPaga = listaContratos.Count(c => c.StatusContrato == StatusEnum.ComissaoPaga),

                // Verifica se há alguma comissão atrasada na lista filtrada
                PossuiComissaoAtrasada = listaContratos.Any(c =>
                    c.StatusContrato == StatusEnum.Pago &&
                    c.DataPgtoContrato.HasValue &&
                    c.DataPgtoContrato.Value < dataLimite),

                // Conta quantos contratos estão com a comissão atrasada na lista filtrada
                ContratosComissaoAtrasada = listaContratos.Count(c =>
                    c.StatusContrato == StatusEnum.Pago &&
                    c.DataPgtoContrato.HasValue &&
                    c.DataPgtoContrato.Value < dataLimite),

                // Envia a lista filtrada para alimentar a tabela    da View
                ListaContratos = listaContratos
            };



            // Retorna a View passando o Dashboard montadinho
            return View(dashboard);
        }

        public IActionResult PorStatus(string status, string busca, int? usuarioId, int pagina = 1)
        {

            ViewBag.Banco = _bancosRepositorio.ListarBancos();


            if (usuarioId == 0)
            {
                usuarioId = null;
            }

            // 1. Buscamos o usuário logado e definimos regras de Admin
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            bool isAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.BuscaAtual = busca;
            ViewBag.StatusParametro = status;

            // 2. Configura a lista de usuários para o dropdown (apenas se for Admin)
            if (isAdmin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos(); // Altere para seu repositório de usuários real se necessário

                // Comportamento padrão inicial do Admin (sem busca ativa e sem clique explícito no dropdown):
                if (usuarioId == null && !Request.Query.ContainsKey("usuarioId") && string.IsNullOrWhiteSpace(busca))
                {
                    usuarioId = usuarioLogado.Id; // Começa exibindo apenas as dele
                }

                ViewBag.UsuarioAtual = usuarioId;
            }


            int tamanhoCorte = 10;
            int totalRegistros = 0;
            List<Venda> vendas;

            // 3. Se houver termo de busca, usa o BuscarCompleto. Se não, usa o ListarTodasVendas original
            if (!string.IsNullOrWhiteSpace(busca))
            {
                vendas = _vendaRepositorio.BuscarCompleto(busca, pagina, tamanhoCorte, out totalRegistros);
            }
            else
            {
                vendas = _vendaRepositorio.ListarTodasVendas();
            }

            // 4. Segurança e Filtros de Usuário:
            if (isAdmin)
            {
                // Se o Admin escolheu filtrar por um usuário específico no dropdown
                if (usuarioId.HasValue)
                {
                    vendas = vendas.Where(v => v.UsuarioId == usuarioId.Value).ToList();
                }
                // Se escolheu "Todos os Usuários" (usuarioId vem nulo, mas chave existe na URL), não filtramos por ID
            }
            else
            {
                // Se não for admin, vê rigidamente apenas as suas próprias vendas
                vendas = vendas.Where(v => v.UsuarioId == usuarioLogado.Id).ToList();
            }


            // 5. Filtragem por Status: Convertemos a string para o Enum
            if (Enum.TryParse(typeof(StatusEnum), status, out var statusEnum))
            {
                var field = statusEnum.GetType().GetField(statusEnum.ToString());
                var attribute = (System.ComponentModel.DataAnnotations.DisplayAttribute)
                    Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DataAnnotations.DisplayAttribute));

                // Enviamos a descrição pronta para o título da View
                ViewBag.StatusAtual = attribute != null ? attribute.Name : status;

                // Aplica o filtro do status selecionado
                vendas = vendas.Where(v => v.StatusContrato == (StatusEnum)statusEnum).ToList();

                // Se não houver busca ativa, fazemos a paginação em memória para manter o ListarTodasVendas intacto
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
                    // Se houver busca, reajusta o total baseado no filtro final de usuários/status
                    totalRegistros = vendas.Count;
                }

                // Dados para os botões de paginação na tela
                ViewBag.PaginaAtual = pagina;
                ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanhoCorte);
                ViewBag.TotalRegistros = totalRegistros;

                return View(vendas);
            }
            else
            {
                TempData["mensagemErro"] = "Status inválido ou não encontrado.";
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

        //[HttpPost]
        //public IActionResult BuscarCliente(string cpf)
        //{
        //    var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
        //    ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

        //    CarregarCombos();

        //    if (string.IsNullOrWhiteSpace(cpf))
        //    {
        //        TempData["MensagemErro"] = "Digite um CPF.";
        //        return View("Criar");
        //    }

        //    var cliente = _clienteRepositorio.BuscarPorCpf(cpf);

        //    if (cliente == null)
        //    {
        //        TempData["MensagemErro"] = "Cliente não cadastrado, para seguir com a venda, favor cadastrar o cliente e o beneficio.";
        //        return View("Criar");
        //    }

        //    if(cliente.ClienteBeneficios == null)
        //    {
        //        TempData["MensagemErro"] = "Não é possivel prosseguir com a venda, cliente não possui beneficio cadastrado";
        //        return View("Criar");
        //    }

        //    var beneficios = _clienteBeneficioRepositorio.ListarBeneficiosPorCliente(cliente.Id);

        //    ViewBag.Cpf = cpf;
        //    ViewBag.ClienteId = cliente.Id;
        //    ViewBag.ClienteNome = cliente.Nome;
        //    ViewBag.Beneficios = beneficios;

        //    return View("Criar");
        //}

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
            if (venda.ValorComissao.HasValue && venda.ValorComissao.Value > 0)
            {
                venda.StatusContrato = StatusEnum.ComissaoPaga; // Altere para StatusContrato se for o nome no seu Model
            }

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

