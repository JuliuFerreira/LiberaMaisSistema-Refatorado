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
        private readonly PermissaoService _permissaoService;

        public ClientesController(IClienteRepositorio clienteRepositorio, IEnderecoRepositorio enderecoRepositorio, ISessao sessao, BancoContext bancoContext, IUsuarioRepositorio usuarioRepositorio, PermissaoService permissaoService)
        {
            _clienteRepositorio = clienteRepositorio;
            _enderecoRepositorio = enderecoRepositorio;
            _sessao = sessao;
            _bancoContext = bancoContext;
            _usuarioRepositorio = usuarioRepositorio;
            _permissaoService = permissaoService;
        }

        public IActionResult Index()
        {
            var usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            ViewBag.IsAdmin = usuarioLogado.Perfil == PerfilEnum.Admin;

            List<Cliente> cliente;

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                cliente = _clienteRepositorio.ListarTodosClientes();
            }

            else
            {
                cliente = _clienteRepositorio.BuscarClientesPorUsuarioId(usuarioLogado.Id);
            }

            return View(cliente);
        }


        public IActionResult Criar()
        {
            var usuarioLogado =
                _sessao.BuscarSessaoDoUsuario();

            ViewBag.IsAdmin =
                usuarioLogado.Perfil == PerfilEnum.Admin;

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                ViewBag.Usuarios =
                    _usuarioRepositorio.ListarTodosUsuarios();
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

                _clienteRepositorio.Adicionar(model.Cliente);

                model.Endereco.ClienteId = model.Cliente.Id;

                _enderecoRepositorio.Adicionar(model.Endereco);

                TempData["MensagemSucesso"] =
                    "Cliente adicionado com sucesso";

                return RedirectToAction("Index");
            }

            catch (Exception)
            {
                TempData["MensagemErro"] =
                    "Não foi possível adicionar o cliente";

                return View(model);
            }
        }

        //catch (Exception ex)

        //{
        //    _bancoContext.Database.RollbackTransaction(); // Caso por algum motivo de erro nos dados do endereço, ele volta ao estado inicial.
        //    throw;

        //}


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

                _clienteRepositorio.Atualizar(clienteDb);

                TempData["MensagemSucesso"] =
                    "Cliente atualizado com sucesso";

                return RedirectToAction("Index");
            }

            catch (Exception)
            {
                TempData["MensagemErro"] =
                    "Não foi possível atualizar o cliente";

                return View(model);
            }
        }


        public IActionResult Deletar(int id)
        {
            var usuarioLogado =_sessao.BuscarSessaoDoUsuario();
            var cliente = _clienteRepositorio.BuscarClientePorId(id);

            if( cliente == null)
            {
                TempData["MensagemErro"] = "Cliente não localizado.";
                return RedirectToAction("Index");
            }

            if(!_permissaoService.UsuarioTemAcessoCliente(usuarioLogado, cliente))
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

                if( cliente == null)
                {
                    TempData["MensagemErro"] = "Cliente não encontrado";
                    return RedirectToAction("Index");
                }

                if(!_permissaoService.UsuarioTemAcessoCliente(usuarioLogado, cliente))
                {
                    TempData["MensagemErro"] = "Você não tem permissão";

                    return RedirectToAction("Index");
                }

                bool apagado = _clienteRepositorio.Apagar(id);

                if (apagado)
                {
                    TempData["mensagemSucesso"] =
                        "Cliente excluido com sucesso";
                    return RedirectToAction("Index");
                }

                else
                {
                    TempData["MensagemErro"] = "Não foi possível excluir o cliente";
                    return RedirectToAction("Index");
                }

            }
            catch (Exception)
            {
                TempData["MensagemErro"] = "Não foi possível excluir o cliente!";
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

