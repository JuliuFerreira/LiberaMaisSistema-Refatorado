using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IOrgaoRepositorio
    {
        public List<Orgao> ListarTodos();

        public Orgao Adicionar(Orgao orgao);

        public Orgao Atualizar(Orgao orgao);

        public Orgao BuscarPorId(int id);

        public bool Apagar(int id);

        public Orgao BuscarPorNome(string nome);


    }
}
