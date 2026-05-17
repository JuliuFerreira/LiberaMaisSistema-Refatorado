using LiberaMais.Models;
using System.Linq;
using LiberaMais.Repositorio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LiberaMais.Models.Enums;
using LiberaMais.Filters;
using LiberaMais.Helper;

namespace LiberaMais.Controllers
{

    [PaginaParaUsuarioLogado]
    public class ClientesController : Controller
    {

        private readonly IClienteRepositorio _clienteRepositorio;
        private readonly ISessao _sessao;

        public ClientesController(IClienteRepositorio clienteRepositorio, ISessao sessao)
        {
            _clienteRepositorio = clienteRepositorio;
            _sessao = sessao;
        }

        public bool IsAniversario(DateTime dataNascimento)
        {
            return dataNascimento.Day == DateTime.Today.Day && dataNascimento.Month == DateTime.Today.Month;
        }

        public IActionResult Clientes(string searchString)
        {
            UsuarioModel usuarioLogado = _sessao.BuscarSessaoDoUsuario();
            List<Cliente> clientes;

            if (usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                clientes = _clienteRepositorio.ListarTodosClientes();
            }
            else
            {
                clientes = _clienteRepositorio.ListarClientes(usuarioLogado.Id);
            }

            // Filtrar clientes com base no searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower(); // Converter para minúsculas
                clientes = clientes.Where(s => (s.UsuarioNome != null && s.UsuarioNome.ToLower().Contains(searchString)) ||
                                               s.Cpf.ToLower().Contains(searchString) ||
                                               s.Nome.ToLower().Contains(searchString)).ToList();
            }

            // Verificar se hoje é o aniversário dos clientes
            List<Cliente> aniversariantes = new List<Cliente>();
            foreach (var cliente in clientes)
            {
                cliente.IsAniversario = IsAniversario(cliente.DataNascimento);
                if (cliente.IsAniversario)
                {
                    aniversariantes.Add(cliente);
                }
            }

            ViewBag.SearchString = searchString;
            ViewBag.TotalClientes = clientes.Count;
            ViewBag.Aniversariantes = aniversariantes;

            return View(clientes);
        }


        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Cliente cliente)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UsuarioModel usuarioModel = _sessao.BuscarSessaoDoUsuario();
                    cliente.UsuarioId = usuarioModel.Id;
                    cliente.UsuarioNome = usuarioModel.Nome;
                    cliente.Nome = cliente.Nome.ToUpper();
                    cliente = _clienteRepositorio.Adicionar(cliente);
                    return Json(new { success = true, message = "Cliente cadastrado com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Ops, não foi possível cadastrar esse cliente!" });
                }
            }
            catch (System.Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível cadastrar o cliente! Detalhe do erro: {erro.Message}" });
            }
        }



        public IActionResult DetalheCliente(long idcliente)
        {
            var edit = _clienteRepositorio.BuscarClientePorId(idcliente);

            return View(edit);
        }

        public IActionResult EditarCliente(long idcliente)
        {
            var cliente = _clienteRepositorio.BuscarClientePorId(idcliente);
                      
            return View(cliente);
        }


        [HttpPost]
        public IActionResult EditarCliente(Cliente cliente)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UsuarioModel usuarioModel = _sessao.BuscarSessaoDoUsuario();
                    cliente.UsuarioId = usuarioModel.Id;
                    cliente.UsuarioNome = usuarioModel.Nome;
                    cliente.Nome = cliente.Nome.ToUpper();
                    cliente = _clienteRepositorio.Atualizar(cliente);
                    return Json(new { success = true, message = "Cliente alterado com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Ops, não foi possível alterar esse cliente!" });
                }
            }
            catch (System.Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível alterar o cliente! Detalhe do erro: {erro.Message}" });
            }
        }

        public IActionResult DeletarCliente(int idcliente)
        {
            var edit = _clienteRepositorio.BuscarClientePorId(idcliente);

            return View(edit);
        }

        public IActionResult Apagar(int idcliente)
        {
            try
            {
                bool apagado = _clienteRepositorio.Apagar(idcliente);

                if (apagado)
                {
                    return Json(new { success = true, message = "Cliente excluído com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Ops, não foi possível excluir o cliente." });
                }
            }
            catch (System.Exception erro)
            {
                return Json(new { success = false, message = $"Ops, não foi possível excluir o cliente. Detalhes do erro: {erro.Message}" });
            }
        }

        [HttpGet]
        public IActionResult VerificarCpfExistente(string cpf)
        {
            if (_clienteRepositorio.VerificarCpfExistente(cpf))
            {
                return Json(false); // CPF já existe
            }

            return Json(true); // CPF não existe
        }

        
    }

}

