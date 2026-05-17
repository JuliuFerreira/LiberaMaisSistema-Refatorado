using LiberaMais.Data;
using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public class PromotorasRepositorio : IPromotorasRepositorio
    {
        private readonly BancoContext _bancoContext;

        public PromotorasRepositorio (BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public Promotora BuscarPromotoraPorId(int id)
        {
            var result = _bancoContext.Promotoras.FirstOrDefault(x => x.Id == id);

            return result;
        }

        public List<Promotora> ListarPromotora()
        {
            return _bancoContext.Promotoras.ToList();

        }

        public Promotora Adicionar(Promotora promotora)
        {
            _bancoContext.Promotoras.Add(promotora);
            _bancoContext.SaveChanges();
            return promotora;
        }

        public Promotora Atualizar(Promotora promotora)
        {
            _bancoContext.Promotoras.Update(promotora);
            _bancoContext.SaveChanges();
            return promotora;
        }

        public bool Apagar(int id)
        {
            Promotora promotora = BuscarPromotoraPorId(id);
            _bancoContext.Promotoras.Remove(promotora);
            _bancoContext.SaveChanges();
            return true;
        }

        public bool VerificarPromotoraExistente(string nome)
        {
            var verificarPromotora = _bancoContext.Promotoras.FirstOrDefault(j => j.Nome == nome);
            
            return verificarPromotora != null;
        }      

        
    }
}
