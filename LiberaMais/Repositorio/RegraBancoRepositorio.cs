using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LiberaMais.Repositorio
{
    public class RegraBancoRepositorio: IRegraBancoRepositorio
    {

        private readonly BancoContext _bancoContext;

        public RegraBancoRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public RegraBanco Adicionar(RegraBanco regraBanco)
        {
            _bancoContext.RegraBanco.Add(regraBanco);
            _bancoContext.SaveChanges();
            return (regraBanco);
        }        

        public RegraBanco Atualizar(RegraBanco regraBanco)
        {
            _bancoContext.RegraBanco.Update(regraBanco);
            _bancoContext.SaveChanges();
            return (regraBanco);
        }

        public RegraBanco BuscarPorId(int id)
        {
            return _bancoContext.RegraBanco
                .Include(r => r.PromotoraBanco)
                .ThenInclude(pb => pb.Promotora)
                .Include(r => r.PromotoraBanco)
                .ThenInclude(pb => pb.Banco)
                .FirstOrDefault(r => r.Id == id); 
        }

        public List<RegraBanco> ListarTodos()
        {
            return _bancoContext.RegraBanco.Include(r => r.PromotoraBanco).ToList();
        }

        public bool Apagar(int id)
        {
            RegraBanco regraBanco = BuscarPorId(id);

            if (regraBanco == null)
            {
                return false;
            }

            _bancoContext.RegraBanco.Remove(regraBanco);
            _bancoContext.SaveChanges();
            return true;
        }

        public List<RegraBanco> BuscarPorNome(string nome, int pagina, int tamanhoPagina, out int totalRegistros)
        {
            var query = _bancoContext.RegraBanco
                .Include(r => r.PromotoraBanco)
                    .ThenInclude(pb => pb.Banco)
                .Include(r => r.PromotoraBanco)
                    .ThenInclude(pb => pb.Promotora)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(nome))
            {
                string nomeLimpo = nome.Trim().ToLower();

                query = query.Where(r =>
                    r.PromotoraBanco.Banco.Nome != null &&
                    r.PromotoraBanco.Banco.Nome.ToLower().Contains(nomeLimpo));
            }


            totalRegistros = query.Count();


            return query
                .OrderByDescending(r => r.DataAtualizacao)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToList();
        }
    }
}
