using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IHistoricoRepositorio
    {
        public List<Historico> ListarTodos();

        public Historico Adicionar(Historico historico);

        public Historico Atualizar (Historico historico);

        public Historico BuscarPorId(int id);

        public List <Historico> ListarPorAcionamento(int acionamentoId);

        bool Apagar(int id);
    }
}
