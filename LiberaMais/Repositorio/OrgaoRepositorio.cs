using LiberaMais.Data;
using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public class OrgaoRepositorio: IOrgaoRepositorio
    {
        private readonly BancoContext _bancoContext;

        public OrgaoRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public Orgao Adicionar(Orgao orgao)
        {
            _bancoContext.orgaos.Add(orgao);
            _bancoContext.SaveChanges();
            return(orgao);
        }
                
        public Orgao Atualizar(Orgao orgao)
        {
            _bancoContext.orgaos.Update(orgao);
            _bancoContext.SaveChanges();
            return (orgao);
        }

        public Orgao BuscarPorId(int id)
        {
            return _bancoContext.orgaos.FirstOrDefault(o => o.Id == id);
        }

        public List<Orgao> ListarTodos()
        {
            return _bancoContext.orgaos.ToList();
        }

        public bool Apagar(int id)
        {
            Orgao orgao = BuscarPorId(id);

            if(orgao == null)
            {
                return false;
            }

            _bancoContext.orgaos.Remove(orgao);
            _bancoContext.SaveChanges();
            return true;
            
        }
    }
}
