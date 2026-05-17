using LiberaMais.Controllers;
using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IClienteRepositorio
    {
        List<Cliente> ListarClientes(int usuarioId);

        Cliente Adicionar(Cliente cliente);

        Cliente BuscarClientePorId(long idcliente);

        Cliente Atualizar(Cliente cliente);

        bool Apagar(int idcliente);

        bool VerificarCpfExistente(string cpf);

        List<Cliente> ListarTodosClientes();

    }
}
