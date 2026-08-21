using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IAciomanentoRepositorio
    {
        public List<Acionamento> ListarTodos();

        public Acionamento BuscarPorId (int id);

        public Acionamento Adicionar(Acionamento acionamento);

        public Acionamento Atualizar(Acionamento acionamento);

        public List<Acionamento> ListaPorUsuario(int usuarioId);

        public List <Acionamento> BuscarPorNomeCpf(string termo, int pagina, int tamanhoCorte, int? usuarioId, out int totalRegistro);

        bool Apagar (int id);

       
    }
}
