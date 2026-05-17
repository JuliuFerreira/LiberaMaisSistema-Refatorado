using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IVendaRepositorio
    {
        Venda BuscarVendaPorId(long id);

        List<Venda> ListarVenda(int usuarioId);

    List<Venda> ListarVendasPorPeriodoEUsuario(DateTime startDate, DateTime endDate, List<int> userIds, string searchString);


        Venda Adicionar(Venda venda);

        Venda Atualizar(Venda venda);

        bool Apagar(int id);

        bool VerificarCpfExistente(string cpf);

        List<Venda> ListarTodasVendas();

        List<Venda> ListarVendasApenasPagos();




    }
}