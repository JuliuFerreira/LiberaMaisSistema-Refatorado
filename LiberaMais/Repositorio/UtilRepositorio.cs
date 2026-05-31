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
    }
}
