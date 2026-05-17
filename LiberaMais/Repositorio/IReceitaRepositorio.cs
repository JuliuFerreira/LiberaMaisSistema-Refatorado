using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IReceitaRepositorio
    {
        Receita BuscarReceitaPorId(int id);

        List<Receita> ListarReceitas(int idFinanca);

        Receita Adicionar(Receita receita);

        Receita Atualizar(Receita receita);

        bool Apagar(int id);
    }
}
