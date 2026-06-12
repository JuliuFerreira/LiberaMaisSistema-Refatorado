using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IBeneficioRepositorio
    {
        public List<Beneficio> ListarTodos();

        public Beneficio Adicionar(Beneficio beneficio);

        public Beneficio Atualizar(Beneficio beneficio);

        public List<Beneficio> BuscarPorOrgao(int orgaoId);

        public bool Apagar(int id);

        public Beneficio BuscarPorId(int id);

    }
}
