using LiberaMais.Data;
using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public class UtilRepositorio : IUtilRepositorio
    {

        private readonly BancoContext _bancoContext;

        public UtilRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public Util Adicionar(Util util)
        {
            _bancoContext.utils.Add(util);
            _bancoContext.SaveChanges();
            return util;
        }

        public Util Atualizar(Util util)
        {
            _bancoContext.utils.Update(util);
            _bancoContext.SaveChanges();
            return util;
        }

        public Util BuscarPorId(int id)
        {
            return _bancoContext.utils.FirstOrDefault(u => u.Id == id);
        }

        public List<Util> ListarTodos()
        {
            return _bancoContext.utils.ToList();
        }

        public bool Apagar(int id)
        {
            Util util = BuscarPorId(id);

            if (util == null)
            {
                return false;
            }

            _bancoContext.utils.Remove(util);
            _bancoContext.SaveChanges();
            return true;
        }

        public List<Util> BuscarPorNome(string nome, int pagina, int tamanhoPagina, out int totalRegistros)
        {
            var query = _bancoContext.utils.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                string nomeLimpo = nome.Trim().ToLower();

                query = query.Where(u =>
                    u.Nome != null &&
                    u.Nome.ToLower().Contains(nomeLimpo));
            }

            var listaAgrupada = query
                .ToList()
                .GroupBy(u => u.Nome)
                .Select(g => g.First())
                .OrderBy(u => u.Nome)
                .ToList();

            totalRegistros = listaAgrupada.Count();

            return listaAgrupada
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToList();
        }
    }
}
