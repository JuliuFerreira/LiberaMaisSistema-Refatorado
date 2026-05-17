using LiberaMais.Controllers;
using LiberaMais.Data;
using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public class DespesaRepositorio : IDespesaRepositorio
    {
        private readonly BancoContext _bancoContext;

        public DespesaRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public Despesa BuscarDespesaPorId(int id)
        {
            var result = _bancoContext.despesas.FirstOrDefault(x => x.Id == id);

            return result;
        }

        public List<Despesa> ListarDespesas(int idFinanca)
        {
            var listarDespesas = _bancoContext.despesas.Where(x => x.FinancaId == idFinanca).ToList();

            if (listarDespesas.Count == 0)
            {
                var despesa = new Despesa()
                {
                    FinancaId = idFinanca,
                };

                listarDespesas.Add(despesa);
            }

            return listarDespesas;
        }

        public Despesa Adicionar(Despesa despesa)
        {
            _bancoContext.despesas.Add(despesa);
            _bancoContext.SaveChanges();
            return despesa;
        }

        public Despesa Atualizar(Despesa despesa)
        {
            _bancoContext.despesas.Update(despesa);
            _bancoContext.SaveChanges();
            return despesa;
         }

        public bool Apagar(int id)
        {
            Despesa despesa = BuscarDespesaPorId(id);

            if (despesa == null) throw new System.Exception("Erro ao excluir a despesa!");

            _bancoContext.despesas.Remove(despesa);
            _bancoContext.SaveChanges();
            return true;
        }

        

        
    }
}
