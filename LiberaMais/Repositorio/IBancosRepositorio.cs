using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IBancosRepositorio
    {
        Banco BuscarBancoPorId(int id);

        Banco Adicionar (Banco banco);

        Banco Atualizar(Banco banco);

        bool Apagar(int id);
        List<Banco> ListarBancos();

        bool VerificarBancoExistente(string nome);

    }
}
