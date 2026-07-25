using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IEnderecoRepositorio

    {

        public List<Endereco> ListarTodos();

        public Endereco BuscarPorId(int id);

        public Endereco Adicionar(Endereco endereco);

        public Endereco Atualizar(Endereco endereco);

        public bool Apagar (int id);

        public void ApagarPorClienteId(int clienteId);


    }
}
