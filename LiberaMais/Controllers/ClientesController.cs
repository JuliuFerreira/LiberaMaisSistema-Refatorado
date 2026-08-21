using LiberaMais.Models;
using System.Linq;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LiberaMais.Models.Enums;
using LiberaMais.Filters;
using LiberaMais.Helper;
using LiberaMais.Data;
using LiberaMais.Services;

namespace LiberaMais.Controllers
{

    [PaginaParaUsuarioLogado]
    public class ClientesController : Controller
    {
        private readonly BancoContext _bancoContext;
        private readonly IClienteRepositorio _clienteRepositorio;
        private readonly IEnderecoRepositorio _enderecoRepositorio;
        private readonly ISessao _sessao;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IClienteBeneficioRepositorio _clienteBeneficioRepositorio;
        private readonly PermissaoService _permissaoService;
        private readonly IVendaRepositorio _vendaRepositorio;
        private readonly IAciomanentoRepositorio _aciomanentoRepositorio;
        private readonly IHistoricoRepositorio _historicoRepositorio;

        public ClientesController(IClienteRepositorio clienteRepositorio,
                          IEnderecoRepositorio enderecoRepositorio,
                          ISessao sessao,
                          BancoContext bancoContext,
                          IUsuarioRepositorio usuarioRepositorio,
                          PermissaoService permissaoService,
                          IClienteBeneficioRepositorio clienteBeneficioRepositorio,
                          IVendaRepositorio vendaRepositorio, 
                          IAciomanentoRepositorio aciomanentoRepositorio,
                          IHistoricoRepositorio historicoRepositorio) 
        {
            _clienteRepositorio = clienteRepositorio;
            _enderecoRepositorio = enderecoRepositorio;
            _sessao = sessao;
            _bancoContext = bancoContext;
            _usuarioRepositorio = usuarioRepositorio;
            _permissaoService = permissaoService;
            _clienteBeneficioRepositorio = clienteBeneficioRepositorio; // Agora vai funcionar perfeitamente!
            _vendaRepositorio = vendaRepositorio;
            _aciomanentoRepositorio = aciomanentoRepositorio;
            _historicoRepositorio = historicoRepositorio;
        }

        public IActionResult Index(string busca, int? usuarioId, int pagina = 1)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            bool isAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.BuscaAtual = busca;


            if(usuarioId == 0)
            {
                usuarioId = null;
            }
            // Carrega a ViewBag dos usuários para preencher o <select> se for Admin
            if (isAdmin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.BuscarTodos(); // Substitua pelo seu método real de listar usuários se necessário

                // Define o comportamento padrão do filtro se for a primeira vez carregando a tela (sem busca e sem filtro explícito)
                if (usuarioId == null && !Request.Query.ContainsKey("usuarioId") && string.IsNullOrWhiteSpace(busca))
                {
                    usuarioId = usuarioLogado.Id; // Padrão: Vendas/Clientes do Admin logado
                }

                ViewBag.UsuarioAtual = usuarioId;

                //ViewBag.TodosUsuario = (usuarioId == null && Request.Query.ContainsKey("usuarioId")) ? "true" : "False";
            }

            int tamanhoPagina = 10;
            int totalRegistros = 0;
            List<Cliente> clientesFinal;

            // 1. SE HOUVER BUSCA
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var resultadoBusca = _clienteRepositorio.BuscarPorNomeOuCpfPaginado(busca, pagina, tamanhoPagina, out totalRegistros);

                if (isAdmin)
                {
                    // Se for Admin e escolheu um usuário específico no select DURANTE a busca
                    if (usuarioId.HasValue)
                    {
                        clientesFinal = resultadoBusca.Where(c => c.UsuarioId == usuarioId.Value).ToList();
                        totalRegistros = _bancoContext.Clientes.Count(c => c.UsuarioId == usuarioId.Value && (c.Nome.Contains(busca) || c.Cpf.Contains(busca)));
                    }
                    else
                    {
                        clientesFinal = resultadoBusca;
                    }
                }
                else
                {
                    clientesFinal = resultadoBusca.Where(c => c.UsuarioId == usuarioLogado.Id).ToList();
                    totalRegistros = _bancoContext.Clientes.Count(c => c.UsuarioId == usuarioLogado.Id && (c.Nome.Contains(busca) || c.Cpf.Contains(busca)));
                }
            }
            // 2. SE NÃO HOUVER BUSCA (Carregamento padrão da sua Index por queryBase)
            else
            {
                var queryBase = _bancoContext.Clientes.Include(c => c.Usuario).AsQueryable();

                if (isAdmin)
                {
                    // Se o Admin selecionou um usuário específico (ou iniciou com o ID dele próprio)
                    if (usuarioId.HasValue)
                    {
                        queryBase = queryBase.Where(c => c.UsuarioId == usuarioId.Value);
                    }
                    // Se clicou em "Todos os Usuários", usuarioId virá nulo mas a chave existirá na URL, não aplicando o Where.
                }
                else
                {
                    // Se não for admin, vê rigidamente apenas os seus registros
                    queryBase = queryBase.Where(c => c.UsuarioId == usuarioLogado.Id);
                }

                totalRegistros = queryBase.Count();

                clientesFinal = queryBase.OrderBy(c => c.Nome)
                                         .Skip((pagina - 1) * tamanhoPagina)
                                         .Take(tamanhoPagina)
                                         .ToList();
            }

            // Passa os dados de controle para os botões da View saberem o que fazer
            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanhoPagina);
            ViewBag.TotalRegistros = totalRegistros;

            return View(clientesFinal);
        }
        //public IActionResult Detalhes(int id)
        //{
        //    var cliente = _clienteRepositorio.BuscarDadosCompletos(id);

        //    if(cliente == null)
        //    {
        //        TempData["MensagemErro"] = "Cliente não encontrado.";
        //        return RedirectToAction("Index");
        //    }

        //    return View(cliente);
        //}

        public IActionResult Criar()
        {
            var usuarioLogado =
                _sessao.BuscarSessaoDoUsuario();

            ViewBag.IsAdmin =
                usuarioLogado.Perfil == PerfilEnum.Admin;

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            }

            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
                    {
                usuarioLogado
                    };
            }

            var model = new ClienteEnderecoViewModel
            {
                Cliente = new Cliente
                {
                    UsuarioId = usuarioLogado.Id
                },


                Endereco = new Endereco()
            };

            return View(model);
        }

        public IActionResult CriarDeAcionamento(int acionamentoId)
        {
            var acionamento = _aciomanentoRepositorio.BuscarPorId(acionamentoId);

            if (acionamento == null)
            {
                TempData["MensagemErro"] = "Beneficiário não localizado.";
                return RedirectToAction("Index", "Acionamento");
            }

            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            }
            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
        {
            usuarioLogado
        };
            }

            var ultimoHistorico = _historicoRepositorio
                .ListarPorAcionamento(acionamentoId)
                .OrderByDescending(x => x.Data)
                .FirstOrDefault();

            var model = new ClienteEnderecoViewModel
            {
                Cliente = new Cliente
                {
                    Nome = acionamento.Nome,
                    Cpf = acionamento.Cpf,
                    UsuarioId = ultimoHistorico?.UsuarioId ?? usuarioLogado.Id,
                    Fone = ultimoHistorico?.Telefone
                },

                Endereco = new Endereco()
            };

            return View("Criar", model);
        }

        [HttpPost]
        public IActionResult Criar(ClienteEnderecoViewModel model)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();

            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            try
            {
                if (usuarioLogado.Perfil == PerfilEnum.Admin)
                {
                    ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
                }

                else
                {
                    ViewBag.Usuarios = new List<UsuarioModel>
                        {
                    usuarioLogado
                        };
                }
                if (!ModelState.IsValid)
                {
                    TempData["MensagemErro"] = "Não foi possível adicionar o cliente";

                    return View(model);
                }

                // Usuário padrão usa automaticamente o próprio Id
                if (usuarioLogado.Perfil != PerfilEnum.Admin)
                {
                    model.Cliente.UsuarioId = usuarioLogado.Id;
                }

                var clienteExistente = _clienteRepositorio.BuscarPorCpf(model.Cliente.Cpf);

                if(clienteExistente != null)
                {
                    TempData["MensagemErro"] = "Já existe um cliente cadastrado com este CPF.";
                    return View(model);
                }

                _clienteRepositorio.Adicionar(model.Cliente);

                model.Endereco.ClienteId = model.Cliente.Id;

                var cliente = _clienteRepositorio.BuscarClientePorId(model.Cliente.Id);

                model.Cliente.Nome = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(model.Cliente.Nome.ToLower());
                _enderecoRepositorio.Adicionar(model.Endereco);

                TempData["MensagemSucesso"] = "Cliente adicionado com sucesso, agora adicione o benefício.  ";

                return RedirectToAction("Criar", "ClienteBeneficio", new { clienteId = cliente.Id });
            }

            catch (Exception)
            {
                TempData["MensagemErro"] = "Não foi possível adicionar o cliente";

                return View(model);
            }
        }


        public IActionResult Detalhes(int id)
        {
            var cliente = _clienteRepositorio.BuscarClientePorId(id);

            if (cliente == null) // se não achar o cliente da erro e volta a index
            {
                TempData["MensagemErro"] = "Cliente não localizado";
                return RedirectToAction("Index");
            }

            var model = new ClienteEnderecoViewModel
            {
                Cliente = cliente,
                Endereco = cliente.Endereco
            };

            return View(model);
        }

        public IActionResult Editar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            var cliente = _clienteRepositorio.BuscarClientePorId(id); // Chama o cliente a ser editado junto com o endereço que ja esta incluido no .Include do repositorio.

            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            if (cliente == null) // se não achar o cliente da erro e volta a index
            {
                TempData["MensagemErro"] = "Cliente não localizado";
                return RedirectToAction("Index");
            }

            if (!_permissaoService.UsuarioTemAcessoCliente(usuarioLogado, cliente))
            {
                TempData["MensagemErro"] = "Você não tem acesso";
                return RedirectToAction("Index");
            }

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios = _usuarioRepositorio.ListarTodosUsuarios();
            }

            else
            {
                ViewBag.Usuarios = new List<UsuarioModel>
                {
                    usuarioLogado
                };
            }
            var model = new ClienteEnderecoViewModel // aqui monta a viewModel para a tela
            {

                Cliente = cliente, // carrega os dados do cliente
                Endereco = cliente.Endereco // carrega os dados do endereço
            };

            return View(model); // retorna a model

        }


        [HttpPost]
        public IActionResult Editar(ClienteEnderecoViewModel model)
        {


            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;
            var clienteDb = _clienteRepositorio.BuscarClientePorId(model.Cliente.Id);

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (clienteDb == null)
                {
                    TempData["MensagemErro"] = "Cliente não encontrado";

                    return RedirectToAction("Index");
                }

                if (!_permissaoService.UsuarioTemAcessoCliente(usuarioLogado, clienteDb))
                {
                    TempData["MensagemErro"] = "Você não tem acesso";
                    return RedirectToAction("Index");
                }

                if (usuarioLogado.Perfil == PerfilEnum.Admin)
                {
                    clienteDb.UsuarioId = model.Cliente.UsuarioId;
                }

                else
                {
                    ViewBag.Usuarios = new List<UsuarioModel>
                    {
                        usuarioLogado
                    };
                }


                clienteDb.Nome =
                model.Cliente.Nome;

                clienteDb.Cpf =
                    model.Cliente.Cpf;

                clienteDb.Fone =
                    model.Cliente.Fone;

                clienteDb.Email =
                    model.Cliente.Email;

                clienteDb.DataNascimento =
                    model.Cliente.DataNascimento;

                clienteDb.Observacoes =
                    model.Cliente.Observacoes;

                clienteDb.Endereco.Cep =
                    model.Endereco.Cep;

                clienteDb.Endereco.Rua =
                    model.Endereco.Rua;

                clienteDb.Endereco.Numero =
                    model.Endereco.Numero;

                clienteDb.Endereco.Bairro =
                    model.Endereco.Bairro;

                clienteDb.Endereco.Cidade =
                    model.Endereco.Cidade;

                clienteDb.Endereco.Estado =
                    model.Endereco.Estado;
                clienteDb.UsuarioId = model.Cliente.UsuarioId;

                clienteDb.Nome = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(clienteDb.Nome.ToLower());

                _clienteRepositorio.Atualizar(clienteDb);

                TempData["MensagemSucesso"] = "Cliente atualizado com sucesso";

                return RedirectToAction("Detalhes", new { id = clienteDb.Id });
            }

            catch (Exception)
            {
                TempData["MensagemErro"] = "Não foi possível atualizar o cliente";

                return View(model);
            }
        }


        public IActionResult Deletar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            var cliente = _clienteRepositorio.BuscarClientePorId(id);


            if (cliente == null)
            {
                TempData["MensagemErro"] = "Cliente não localizado.";
                return RedirectToAction("Index");
            }

            if (!_permissaoService.UsuarioTemAcessoCliente(usuarioLogado, cliente))
            {
                TempData["MensagemErro"] = "Você não tem permissão";

                return RedirectToAction("Index");
            }

            return View(cliente);

        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            var cliente = _clienteRepositorio.BuscarClientePorId(id);

            try
            {
                if (cliente == null)
                {
                    TempData["MensagemErro"] = "Cliente não encontrado";
                    return RedirectToAction("Index");
                }

                if (!_permissaoService.UsuarioTemAcessoCliente(usuarioLogado, cliente))
                {
                    TempData["MensagemErro"] = "Você não tem permissão";
                    return RedirectToAction("Index");
                }

                bool pussuiVenda = _bancoContext.Venda.Any(v => v.ClienteId == id);

                if (pussuiVenda)
                {
                    TempData["MensagemErro"] = "Não é possível excluir o cliente, pois existem vendas cadastradas em seu nome.";
                    TempData.Keep("MensagemErro");
                    return RedirectToAction("Index");
                }

                // Deixamos o trabalho pesado para o repositório do cliente, evitando erros de injeção de dependência
                bool apagado = _clienteRepositorio.Apagar(id);

                if (apagado)
                {
                    TempData["mensagemSucesso"] = "Cliente excluído com sucesso";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["MensagemErro"] = "Não foi possível excluir o cliente";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && (ex.InnerException.Message.Contains("FOREIGN KEY") || ex.InnerException.Message.Contains("REFERENCE")))
                {
                    TempData["MensagemErro"] = "Não é possível excluir o cliente, pois existem registros (Vendas) vinculados a ele.";
                }
                else
                {
                    TempData["MensagemErro"] = "Erro ao excluir: " + ex.GetBaseException().Message;
                }

                TempData.Keep("MensagemErro");
                return RedirectToAction("Index");
            }
        }
        //[HttpGet]
        //public IActionResult VerificarCpfExistente(string cpf)
        //{
        //    if (_clienteRepositorio.VerificarCpfExistente(cpf))
        //    {
        //        return Json(false); // CPF já existe
        //    }

        //    return Json(true); // CPF não existe
        //}


    }

}

