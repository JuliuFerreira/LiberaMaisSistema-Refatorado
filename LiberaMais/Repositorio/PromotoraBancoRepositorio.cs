
using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Repositorio
{
    public class PromotoraBancoRepositorio : IPromotoraBancoRepositorio
    {

        private readonly BancoContext _bancoContext;

        public PromotoraBancoRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public PromotoraBanco Adicionar(PromotoraBanco promotoraBanco)
        {
            _bancoContext.PromotoraBancos.Add(promotoraBanco);
            _bancoContext.SaveChanges();
            return promotoraBanco;
        }

        public bool Apagar(int id)
        {

            PromotoraBanco promotoraBanco = BuscarPorId(id);
            if (promotoraBanco == null)
            {
                return false;
            }
            _bancoContext.PromotoraBancos.Remove(promotoraBanco);
            _bancoContext.SaveChanges();
            return true;

        }

        public PromotoraBanco Atualizar(PromotoraBanco promotoraBancoDb)
        {
            _bancoContext.PromotoraBancos.Update(promotoraBancoDb);
            _bancoContext.SaveChanges();
            return promotoraBancoDb;
        }

        public PromotoraBanco BuscarPorId(int id)
        {
            var promotoraBanco = _bancoContext.PromotoraBancos.
            Include(x  => x.Banco).
            Include(x => x.Promotora).
            FirstOrDefault(x => x.Id == id);
            return promotoraBanco;
        }

        public List<PromotoraBanco> ListarPromotoraBanco()
        {
            return _bancoContext.PromotoraBancos.
            Include(x => x.Banco).
            Include(x => x.Promotora).
            ToList();
        }

        public List<PromotoraBanco> ListarPorPromotora(int promotoraId)
        {
            return _bancoContext.PromotoraBancos
                .Include(pb => pb.Promotora)
                .Include(pb => pb.Banco)
                .Where(pb => pb.PromotoraId == promotoraId)
                .ToList();
        }
    }
}
