using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Repositorio
{
    public class AgendamentoRepositorio : IAgendamentoRepositorio
    {
        private readonly BancoContext _bancoContext;

        public AgendamentoRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public List<Agendamento> ListarTodos()
        {
            return _bancoContext.Agendamento
                 .Include(a => a.Cliente)
                 .Include(a => a.Usuario)
                 .ToList();
        }

        public Agendamento BuscarPorId(int id)
        {
            return _bancoContext.Agendamento
                .Include(a => a.Cliente)
                .Include(a => a.Usuario)
                .FirstOrDefault(a => a.Id == id);
        }

        public Agendamento Adicionar(Agendamento agendamento)
        {
            _bancoContext.Agendamento.Add(agendamento);
            _bancoContext.SaveChanges();
            return (agendamento);
        }

        public Agendamento Atualizar(Agendamento agendamento)
        {
            _bancoContext.Agendamento.Update(agendamento);
            _bancoContext.SaveChanges();
            return (agendamento);
        }

        public bool Apagar(int id)
        {
            Agendamento agendamento = BuscarPorId(id);

            if(agendamento == null)
            {
                return false;
            }

            _bancoContext.Agendamento.Remove(agendamento);
            _bancoContext.SaveChanges();
            return true;
        }

        public List<Agendamento> BuscarCompleto(string termo, int pagina, int tamanhoCorte, int? usuarioId, out int totalRegistros)
        {
            var query = _bancoContext.Agendamento
                .Include(a => a.Cliente)
                .Include(a => a.Usuario)
                .AsQueryable();

            if (usuarioId.HasValue)
            {
                query = query.Where(a => a.UsuarioId == usuarioId.Value);
            }

            if (!string.IsNullOrWhiteSpace(termo))
            {
                string termoLimpo = termo.Replace(".", "").Replace("-", "").Trim().ToLower();
                query = query.Where(a => (a.Cliente.Nome != null && a.Cliente.Nome.ToLower().Contains(termoLimpo)) ||
                                   (a.Cliente.Cpf != null && a.Cliente.Cpf.Replace(".","").Replace("-", "").Contains(termoLimpo)));
            }

            totalRegistros = query.Count();

            return query.OrderByDescending(a => a.DataCadastro)
                .Skip((pagina - 1) * (tamanhoCorte))
                .Take(tamanhoCorte).ToList();
        }

        public List<Cliente> BuscarCliente(string termo)
        {
            var query = _bancoContext.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                string termoLimpo = termo.Replace(".", "").Replace("-", "").Trim().ToLower();
                query = query.Where(c => (c.Nome != null && c.Nome.ToLower().Contains(termoLimpo)) ||
                                         (c.Cpf != null && c.Cpf.Replace(".", "").Replace("-", "").Contains(termoLimpo)));
            }

            return query
                .OrderBy(c => c.Nome)
                .ToList();
        }

        public List<Agendamento> BuscarPorUsuario(int usuarioId)
        {
            return _bancoContext.Agendamento
                .Include(a => a.Cliente)
                .Include(a => a.Usuario)
                .Where(a => a.UsuarioId == usuarioId).ToList();
        }
    }
}
