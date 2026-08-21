using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IUtilRepositorio
    {
        public List<Util> ListarTodos();

        public Util BuscarPorId(int id);

        public Util Adicionar(Util util);

        public Util Atualizar(Util util);

        public bool Apagar (int id);

        public List<Util> BuscarPorNome(string nome, int pagina, int tamanhoCorte, out int totalRegistros);
    }
}
