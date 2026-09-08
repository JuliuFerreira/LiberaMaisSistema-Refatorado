using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IAgendamentoRepositorio
    {

        public List<Agendamento> ListarTodos();

        public Agendamento Adicionar(Agendamento agendamento);

        public Agendamento Atualizar(Agendamento agendamento);

        public Agendamento BuscarPorId(int id);

        public List<Agendamento> BuscarCompleto(string termo, int pagina, int tamanhoCorte, int? usuarioId, out int totalRegistros);

        public List<Cliente> BuscarCliente(string termo);

        public List<Agendamento> BuscarPorUsuario(int usuarioId);

        bool Apagar(int id);
    }
}
