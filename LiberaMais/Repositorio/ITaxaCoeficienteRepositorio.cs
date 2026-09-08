using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface ITaxaCoeficienteRepositorio
    {
        public List<TaxaCoeficiente> ListarTodos();

        public TaxaCoeficiente Adicionar(TaxaCoeficiente taxaCoeficiente);

        public TaxaCoeficiente Atualizar(TaxaCoeficiente taxaCoeficiente);

        public TaxaCoeficiente BuscarPorId(int id);

        public bool Apagar(int id);


    }
}
