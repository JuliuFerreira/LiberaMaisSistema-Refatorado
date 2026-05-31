using LiberaMais.Controllers;
using LiberaMais.Data;
using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IClienteRepositorio
    {

        Cliente Adicionar(Cliente cliente); /*BancoContext bancoContext*/

        Cliente BuscarClientePorId(int id);

        Cliente Atualizar(Cliente cliente);

        bool Apagar(int id);

       // bool VerificarCpfExistente(string cpf);

        List<Cliente> ListarTodosClientes();

        List<Cliente> BuscarClientesPorUsuarioId(int usuarioId);

    }
}
