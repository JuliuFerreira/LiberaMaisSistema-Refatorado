using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Repositorio
{
    public class BeneficioRepositorio: IBeneficioRepositorio
    {
        private readonly BancoContext _bancoContext;

        public BeneficioRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public List<Beneficio> ListarTodos()
        {
            return _bancoContext.beneficios
            .Include(b => b.Orgaos)
            .ToList();
        }

        public Beneficio Adicionar(Beneficio beneficio)
        {
            _bancoContext.beneficios.Add(beneficio);
            _bancoContext.SaveChanges();
            return beneficio;
        }

        public Beneficio Atualizar(Beneficio beneficio)
        {
            _bancoContext.beneficios.Update(beneficio);
            _bancoContext.SaveChanges();
            return beneficio;
        }

        public Beneficio BuscarPorId(int id)
        {
            return _bancoContext.beneficios
            .Include(b => b.Orgaos)
            .FirstOrDefault(b => b.Id == id);
            
        }

        public bool Apagar(int id)
        {
            Beneficio beneficio = BuscarPorId(id);

            if(beneficio == null)
            {
                return false;
            }

            _bancoContext.beneficios.Remove(beneficio);
            _bancoContext.SaveChanges();
            return true;
        }

        public List<Beneficio> BuscarPorOrgao(int orgaoId)
        {
            return _bancoContext.beneficios
                .Where(x => x.OrgaoId == orgaoId)
                .ToList();
        }
    }
}
