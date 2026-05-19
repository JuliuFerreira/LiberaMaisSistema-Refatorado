using LiberaMais.Data;
using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IPromotorasRepositorio
    {
        Promotora BuscarPromotoraPorId(int id);

        List<Promotora> ListarPromotora();

        List<Promotora> ListarPorUsuario(int usuarioId);

        Promotora Adicionar(Promotora promotora);

        Promotora Atualizar(Promotora promotora);

        bool Apagar(int id);

        bool VerificarPromotoraExistente(string nome);
    }
}
