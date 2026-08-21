using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Repositorio
{
    public class HistoricoRepositorio : IHistoricoRepositorio
    {
        private readonly BancoContext _bancoContext;

        public HistoricoRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public Historico Adicionar(Historico historico)
        {
            _bancoContext.Historico.Add(historico);
            _bancoContext.SaveChanges();
            return historico;
        }

        public Historico BuscarPorId(int id)
        {
            return _bancoContext.Historico
                .Include(h => h.Acionamento)
                .FirstOrDefault(h => h.Id == id);
        }

        public Historico Atualizar(Historico historico)
        {
            _bancoContext.Historico.Update(historico);
            _bancoContext.SaveChanges();
            return historico;

        }

        public List<Historico> ListarTodos()
        {
            return _bancoContext.Historico
                .Include(h => h.Acionamento)
                .ToList();
        }

        public bool Apagar(int id)
        {
            Historico historico = BuscarPorId(id);

            if (historico == null)
            {
                return false;
            }

            _bancoContext.Historico.Remove(historico);
            _bancoContext.SaveChanges();
            return true;
        }

        public List<Historico> ListarPorAcionamento(int acionamentoId)
        {
            return _bancoContext.Historico
                .Include(h => h.Usuario)
                .Where(h => h.AcionamentoId == acionamentoId)
                .OrderByDescending(h => h.Data)
                .ToList();
        }
    }
}


