using AspNetCoreGeneratedDocument;
using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IVendaRepositorio
    {
        List<Venda> ListarTodasVendas();

        Venda BuscarVendaPorId(int id);

        Venda Adicionar(Venda venda);

        Venda Atualizar(Venda venda);

        bool Apagar(int id);

        List<Venda> ListarVendasPorUsuario(int usuarioId);

        List<Venda> BuscarVendasPagasPorPeriodo(int mes, int ano);

    }
}