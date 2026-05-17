using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IFinancaRepositorio
    {
        Financa BuscarMesAnoPorId(int id);
        List<Financa> ListaFinanca();
        Financa Adicionar(Financa financa);
        Financa Atualizar(Financa financa);
        bool Apagar(int id);
        bool ExisteMesEAno(int mes, int ano);
        decimal CalcularTotalReceitas(int financaId);
        decimal CalcularTotalDespesas(int financaId);
        IQueryable<Receita> GetReceitas();
        IQueryable<Despesa> GetDespesas();
    }
}
