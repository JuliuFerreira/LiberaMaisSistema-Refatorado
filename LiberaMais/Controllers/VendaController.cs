using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Models;
using LiberaMais.Models.Enums;
using LiberaMais.Repositorio;
using LiberaMais.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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


        public IActionResult Index()
        {
            // Busca o usuário logado para sabermos se ele é Admin ou operador comum
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            // CHAMADA DO REPOSITÓRIO: 
            // Buscamos todos os contratos do banco através do repositório que criamos!
            var listaContratos = _vendaRepositorio.ListarTodasVendas();

            // SEGURANÇA: Se não for Admin, filtramos a lista para mostrar apenas os 
            // contratos cujo UsuarioId da Venda Mãe seja igual ao ID do usuário logado
            if (usuarioLogado.Perfil != PerfilEnum.Admin)
            {
                listaContratos = listaContratos
                    .Where(c => c.UsuarioId == usuarioLogado.Id)
                    .ToList();
            }

            // REGRA DE NEGÓCIO: Calcula a data limite para comissão atrasada (Hoje menos 5 dias)
            var dataLimite = DateTime.Now.AddDays(-5);

            // Montamos o seu Dashboard contando os status diretamente da lista de contratos filhos
            DashboardVendaViewModel dashboard = new DashboardVendaViewModel
            {
                Digitado = listaContratos.Count(c => c.StatusContrato == StatusEnum.Digitado),
                Assinado = listaContratos.Count(c => c.StatusContrato == StatusEnum.Assinado),
                Pago = listaContratos.Count(c => c.StatusContrato == StatusEnum.Pago),
                Cancelado = listaContratos.Count(c => c.StatusContrato == StatusEnum.Cancelado),
                ComissaoPaga = listaContratos.Count(c => c.StatusContrato == StatusEnum.ComissaoPaga),

                // Verifica se há alguma comissão atrasada na lista
                PossuiComissaoAtrasada = listaContratos.Any(c =>
                    c.StatusContrato == StatusEnum.Pago &&
                    c.DataPgtoContrato.HasValue &&
                    c.DataPgtoContrato.Value < dataLimite),

                // Conta quantos contratos estão com a comissão atrasada
                ContratosComissaoAtrasada = listaContratos.Count(c =>
                    c.StatusContrato == StatusEnum.Pago &&
                    c.DataPgtoContrato.HasValue &&
                    c.DataPgtoContrato.Value < dataLimite),

                // ENVIAMOS A LISTA: Colocamos a lista de contratos dentro do ViewModel 
                // para a tabela da View conseguir ler e exibir na tela!
                ListaContratos = listaContratos
            };

            // Retorna a View passando o Dashboard montadinho
            return View(dashboard);
        }

        public IActionResult PorStatus(string status)
        {

            ViewBag.Banco = _bancosRepositorio.ListarBancos();

            // 1. Buscamos o usuário logado
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            // Passamos o nome do status para a View exibir
            ViewBag.StatusAtual = status;

            // 2. Buscamos TODAS as Vendas através do repositório de Venda
            // Certifique-se de usar o repositório correto aqui
            var vendas = _vendaRepositorio.ListarTodasVendas();

            // 3. Segurança: Se não for Admin, filtramos para o operador só ver o que ele digitou
            if (usuarioLogado.Perfil != PerfilEnum.Admin)
            {
                vendas = vendas.Where(v => v.UsuarioId == usuarioLogado.Id).ToList();
            }

            // 4. Filtragem por Status: Convertemos a string para o Enum
            if (Enum.TryParse(typeof(StatusEnum), status, out var statusEnum))
            {
                // Filtramos a lista de vendas pelo status
                vendas = vendas.Where(v => v.StatusContrato == (StatusEnum)statusEnum).ToList();
            }
            else
            {
                TempData["mensagemErro"] = "Status inválido ou não encontrado.";
                return RedirectToAction("Index");
            }

            // 5. Retorna a View passando a lista de Vendas
            return View(vendas);
        }


        [HttpPost]
        public IActionResult BuscarCliente(string cpf)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            CarregarCombos();

            if (string.IsNullOrWhiteSpace(cpf))
            {
                TempData["MensagemErro"] = "Digite um CPF.";
                return View("Criar");
            }

            var cliente = _clienteRepositorio.BuscarPorCpf(cpf);

            if (cliente == null)
            {
                TempData["MensagemErro"] = "Cliente não cadastrado, para seguir com a venda, favor cadastrar o cliente e o beneficio.";
                return View("Criar");
            }

            var beneficios = _clienteBeneficioRepositorio.ListarBeneficiosPorCliente(cliente.Id);

            ViewBag.Cpf = cpf;
            ViewBag.ClienteId = cliente.Id;
            ViewBag.ClienteNome = cliente.Nome;
            ViewBag.Beneficios = beneficios;

            return View("Criar");
        }

        public IActionResult Criar()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            CarregarCombos();

            return View();
        }

        [HttpPost]
        public IActionResult Criar(Venda venda)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            bool isAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if (!isAdmin)
            {
                venda.UsuarioId = usuarioLogado.Id;
            }

            // 1. CORREÇÃO: Se der erro, precisamos validar o ModelState.
            // Camadas de portabilidade podem disparar erros fantasmas se forem nulas na operação normal.
            if (!ModelState.IsValid)
            {
                // Força o recarregamento das ViewBags do Cliente baseado no ID que veio no formulário
                if (venda.ClienteId > 0)
                {
                    var cliente = _clienteRepositorio.BuscarClientePorId(venda.ClienteId); // Ajuste o nome do seu repositório de cliente se necessário
                    if (cliente != null)
                    {
                        ViewBag.Cpf = cliente.Cpf;
                        ViewBag.ClienteNome = cliente.Nome;
                        // Caso seu repositório traga os benefícios mapeados:
                        ViewBag.Beneficios = cliente.ClienteBeneficios;
                    }
                }

                // Recarrega os dados essenciais da tela
                ViewBag.IsAdmin = isAdmin;
                CarregarCombos();

                // CAPTURAR O ERRO DE VALIDAÇÃO (Opcional, mas ajuda muito a debugar)
                var erros = string.Join(" | ", ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage));
                TempData["MensagemErro"] = "Dados inválidos no formulário: " + erros;

                return View(venda);
            }

            try
            {
                // Regra de negócio simples
                if (venda.ValorComissao.HasValue && venda.ValorComissao.Value > 0)
                {
                    venda.StatusContrato = StatusEnum.ComissaoPaga;
                }

                _vendaRepositorio.Adicionar(venda);

                TempData["MensagemSucesso"] = "Venda cadastrada com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao salvar: " + ex.Message;

                // Mantém os dados caso caia no banco de dados
                ViewBag.IsAdmin = isAdmin;
                if (venda.ClienteId > 0)
                {
                    var cliente = _clienteRepositorio.BuscarClientePorId(venda.ClienteId);
                    if (cliente != null)
                    {
                        ViewBag.Cpf = cliente.Cpf;
                        ViewBag.ClienteNome = cliente.Nome;
                        ViewBag.Beneficios = cliente.ClienteBeneficios;
                    }
                }

                CarregarCombos();
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
        [HttpGet]
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

            var vendaDb = _vendaRepositorio.BuscarVendaPorId(venda.Id);

            if (vendaDb == null)
            {
                TempData["MensagemErro"] = "Venda não localizada.";
                return RedirectToAction("Index");
            }

            if (!_permissaoService.UsuarioTemAcessoVenda(usuarioLogado, vendaDb))
            {
                TempData["MensagemErro"] = "Você não tem acesso a esta venda.";
                return RedirectToAction("Index");
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

                TempData["MensagemErro"] = "Existem campos inválidos no formulário.";
                return View(venda);
            }

            // --- REGRA DE NEGÓCIO DA COMISSÃO ---
            // Se o valor da comissão foi preenchido e é maior que zero, força o status para ComissaoPaga
            if (venda.ValorComissao.HasValue && venda.ValorComissao.Value > 0)
            {
                venda.StatusContrato     = StatusEnum.ComissaoPaga; // Altere para StatusContrato se for o nome no seu Model
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
                return RedirectToAction("PorStatus");
            }

            if (!_permissaoService.UsuarioTemAcessoVenda(usuarioLogado, venda))
            {
                TempData["MensagemErro"] = "Você não tem acesso";
                return RedirectToAction("PorStatus");
            }

            // Guardamos o status da venda antes de excluí-la para saber para onde voltar
            var statusDaVenda = venda.StatusContrato;

            try
            {
                bool apagado = _vendaRepositorio.Apagar(id);

                if (apagado)
                {
                    TempData["MensagemSucesso"] = "Venda apagada com sucesso.";

                    // Enviamos o parâmetro que a sua Action "PorStatus" precisa receber (ajuste o nome 'status' se a sua Action usar outro nome de variável)
                    return RedirectToAction("PorStatus", new { status = statusDaVenda });
                }
                else
                {
                    TempData["MensagemErro"] = "Não é possível apagar essa venda.";
                    return RedirectToAction("PorStatus", new { status = statusDaVenda });
                }
            }
            catch (Exception)
            {
                TempData["MensagemErro"] = "Não foi possível excluir a venda!";
                return RedirectToAction("PorStatus", new { status = statusDaVenda });
            }
        }
    }


}

