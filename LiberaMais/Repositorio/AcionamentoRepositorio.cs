using LiberaMais.Controllers;
using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Repositorio
{
    public class AcionamentoRepositorio : IAciomanentoRepositorio
    {
        private readonly BancoContext _bancoContext;


        public AcionamentoRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public Acionamento Adicionar(Acionamento acionamento)
        {
            _bancoContext.Acionamento.Add(acionamento);
            _bancoContext.SaveChanges();
            return (acionamento);
        }

        public bool Apagar(int id)
        {
            var acionamento = _bancoContext.Acionamento
                .FirstOrDefault(a => a.Id == id);

            if (acionamento == null)
            {
                return false;
            }

            var possuiHistorico = _bancoContext.Historico
                .Any(h => h.AcionamentoId == id);

            if (possuiHistorico)
            {
                return false;
            }

            _bancoContext.Acionamento.Remove(acionamento);

            _bancoContext.SaveChanges();

            return true;
        }

        public Acionamento BuscarPorId(int id)
        {
            return _bancoContext.Acionamento
                .Include(ac => ac.Usuario)
                 .FirstOrDefault(ac => ac.Id == id);
        }

        public Acionamento Atualizar(Acionamento acionamento)
        {
            _bancoContext.Acionamento.Update(acionamento);
            _bancoContext.SaveChanges();
            return (acionamento);
        }

        public List<Acionamento> ListaPorUsuario(int usuarioId)
        {
            return _bancoContext.Acionamento
                .Include(ac => ac.Usuario)
                .Where(ac => ac.UsuarioId == usuarioId).ToList();
        }

        public List<Acionamento> ListarTodos()
        {
            return _bancoContext.Acionamento
                  .Include(ac => ac.Historicos)
                  .Include(ac => ac.Usuario)
                .ToList();
        }

        public List <Acionamento> BuscarPorNomeCpf(string termo, int pagina ,int tamanhoCorte, int? usuarioId, out int totalRegistro)
        {
            var query = _bancoContext.Acionamento.AsQueryable();

            if (usuarioId.HasValue)
            {
                query = query.Where(u => u.UsuarioId == usuarioId.Value);
            }

            if (!string.IsNullOrWhiteSpace(termo))
            {
                string termoLimpo = termo.Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Trim().ToLower();

                query = query.Where(c => (c.Nome != null && c.Nome.ToLower().Contains(termoLimpo)) ||
                                   (c.Cpf != null && c.Cpf.Replace(".", "").Replace("-", "").Contains(termoLimpo)));
            }

            totalRegistro = query.Count();

            return query.OrderByDescending(c => c.DataCadastro)
                         .Skip((pagina - 1) * tamanhoCorte)
                         .Take(tamanhoCorte)
                         .ToList();

        }
    }
}
