using LiberaMais.Data;
using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IPromotorasRepositorio
    {
        Promotora BuscarPromotoraPorId(int id);

        List<Promotora> ListarPromotora();

        Promotora Adicionar(Promotora promotora);

        Promotora Atualizar(Promotora promotora);

        bool Apagar(int id);

        bool VerificarPromotoraExistente(string nome);
    }
}
