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

        Cliente BuscarCompleto(int id);

        bool Apagar(int id);

        public Cliente BuscarPorCpf(string cpf);

       // bool VerificarCpfExistente(string cpf);

        List<Cliente> ListarTodosClientes();

        List<Cliente> BuscarClientesPorUsuarioId(int usuarioId);

        List<Cliente> BuscarPorNomeOuCpfPaginado(string termo, int pagina, int tamanhoCorte, out int totalRegistros);

    }
}
