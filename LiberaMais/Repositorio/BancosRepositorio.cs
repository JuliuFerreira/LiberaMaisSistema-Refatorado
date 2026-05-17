using LiberaMais.Data;
using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public class BancosRepositorio : IBancosRepositorio
    {
        private readonly BancoContext _bancoContext;

        public BancosRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public Banco BuscarBancoPorId(int id)
        {
            var result = _bancoContext.Bancos.FirstOrDefault(b => b.Id == id);

            return result;
        }

        public List<Banco> ListarBancos()
        {
           return _bancoContext.Bancos.ToList();
        }

        public Banco Adicionar(Banco banco)
        {
            _bancoContext.Bancos.Add(banco);
            _bancoContext.SaveChanges();
            return banco;
        }

        public Banco Atualizar(Banco banco)
        {
            _bancoContext.Bancos.Update(banco);
            _bancoContext.SaveChanges();
            return banco;
            
        }

        public bool Apagar(int id)
        {
            Banco banco = BuscarBancoPorId(id);

            if (banco == null) throw new System.Exception("Erro ao excluir o login");

            _bancoContext.Bancos.Remove(banco);
            _bancoContext.SaveChanges();
            return true;
        }


        public bool VerificarBancoExistente(string nome)
        {
            var verificarBanco = _bancoContext.Bancos.FirstOrDefault(b => b.Nome == nome);
            return verificarBanco == null;
        }
    }
}
