using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IFinancaRepositorio
    {
        List<Financa> BuscarTodos(int usuarioId, int mes, int ano);

        List<Financa> ListarPorPeriodo(int mes, int ano, int? usuarioId);
        Financa Adicionar(Financa financa);
        Financa Atualizar(Financa financa);
        bool Apagar(int id);
        Financa BuscarPorId(int id);
 
    }
}
