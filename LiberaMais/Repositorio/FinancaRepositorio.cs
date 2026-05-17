using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Repositorio
{
    public class FinancaRepositorio : IFinancaRepositorio
    {
        private readonly BancoContext _bancoContext;

        public FinancaRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public IQueryable<Receita> GetReceitas()
        {
            return _bancoContext.receitas.AsQueryable();
        }

        public IQueryable<Despesa> GetDespesas()
        {
            return _bancoContext.despesas.AsQueryable();
        }

        public Financa BuscarMesAnoPorId(int id)
        {
            var result = _bancoContext.financas
        .Include(f => f.Receitas)
        .Include(f => f.Despesas)
        .FirstOrDefault(x => x.Id == id);

            return result;
        }

        public List<Financa> ListaFinanca()
        {
            var listarFinanca = _bancoContext.financas
                .Include(f => f.Receitas)
                .Include(f => f.Despesas)
                .ToList();

            foreach (var financa in listarFinanca)
            {
                financa.TotalReceitas = CalcularTotalReceitas(financa.Id);
                financa.TotalDespesas = CalcularTotalDespesas(financa.Id);
            }

            return listarFinanca;
        }

        public bool ExisteMesEAno(int mes, int ano)
        {
            return _bancoContext.financas.Any(f => f.Mes == mes && f.Ano == ano);
        }

        public Financa Adicionar(Financa financa)
        {
            financa.TotalReceitas = 0;
            financa.TotalDespesas = 0;

            _bancoContext.financas.Add(financa);
            _bancoContext.SaveChanges();
            return financa;
        }

        public Financa Atualizar(Financa financa)
        {
            _bancoContext.financas.Update(financa);
            _bancoContext.SaveChanges();
            return financa;

            if (financa == null) throw new System.Exception("Erro ao atualizar o mês/ano!");
        }

        public bool Apagar(int id)
        {
            Financa financa = BuscarMesAnoPorId(id);

            if (financa == null) throw new System.Exception("Erro ao excluir o mês/ano!");

            _bancoContext.financas.Remove(financa);
            _bancoContext.SaveChanges();
            return true;
        }

        public decimal CalcularTotalReceitas(int financaId)
        {
            return _bancoContext.receitas
                .Where(r => r.FinancaId == financaId)
                .Sum(r => r.ValorRecebido);
        }

        public decimal CalcularTotalDespesas(int financaId)
        {
            return _bancoContext.despesas
                .Where(d => d.FinancaId == financaId)
                .Sum(d => d.ValorDespesa);
        }
    }
}
