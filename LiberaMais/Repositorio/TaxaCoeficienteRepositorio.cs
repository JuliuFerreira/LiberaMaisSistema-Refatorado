using LiberaMais.Data;
using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public class TaxaCoeficienteRepositorio : ITaxaCoeficienteRepositorio
    {
        private readonly BancoContext _bancoContext;

        public TaxaCoeficienteRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public List<TaxaCoeficiente> ListarTodos()
        {
            return _bancoContext.TaxaCoeficiente.ToList();
        }

        public TaxaCoeficiente Adicionar(TaxaCoeficiente taxaCoeficiente)
        {
            _bancoContext.TaxaCoeficiente.Add(taxaCoeficiente);
            _bancoContext.SaveChanges();
            return taxaCoeficiente;
        }        

        public TaxaCoeficiente Atualizar(TaxaCoeficiente taxaCoeficiente)
        {
            _bancoContext.TaxaCoeficiente.Update(taxaCoeficiente);
            _bancoContext.SaveChanges();
            return taxaCoeficiente;
        }

        public TaxaCoeficiente BuscarPorId(int id)
        {
            return _bancoContext.TaxaCoeficiente.FirstOrDefault(t => t.Id == id);
        }
        

        public bool Apagar(int id)
        {

            var apagado = BuscarPorId(id);

            if(apagado == null)
            {
                return false;
            }

            _bancoContext.TaxaCoeficiente.Remove(apagado);
            _bancoContext.SaveChanges();
            return true;
        }
    }
}
