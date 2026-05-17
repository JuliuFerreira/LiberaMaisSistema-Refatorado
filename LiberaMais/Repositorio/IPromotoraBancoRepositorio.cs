using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IPromotoraBancoRepositorio
    {
        List<PromotoraBanco> ListarPromotoraBanco();

        List<PromotoraBanco> ListarPorPromotora(int promotoraId);

        PromotoraBanco Adicionar(PromotoraBanco promotoraBanco);

        PromotoraBanco Atualizar(PromotoraBanco promotoraBanco);

        bool Apagar(int id);

        PromotoraBanco BuscarPorId(int id);
    }
}
