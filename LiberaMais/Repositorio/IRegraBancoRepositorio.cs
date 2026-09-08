using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IRegraBancoRepositorio
    {
        public List<RegraBanco> ListarTodos();

        public RegraBanco Adicionar(RegraBanco regraBanco);

        public RegraBanco Atualizar(RegraBanco regraBanco);

        public RegraBanco BuscarPorId(int id);

        public List<RegraBanco> BuscarPorNome(string nome, int pagina, int tamanhoPagina, out int totalRegistros);

        bool Apagar(int id);
    }
}
