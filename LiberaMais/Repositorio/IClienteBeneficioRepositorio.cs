using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IClienteBeneficioRepositorio
    {
        public List<ClienteBeneficio> ListarTodos();


        public ClienteBeneficio Adicionar(ClienteBeneficio clienteBeneficio);

        public ClienteBeneficio Atualizar(ClienteBeneficio clienteBeneficio);

        public ClienteBeneficio BuscarPorId(int id);

        public bool Apagar(int id);

        List<ClienteBeneficio> ListarBeneficiosPorCliente(int clienteId);

        public void ApagarPorClienteId(int clienteId);

        List<ClienteBeneficio> BuscarPorNomeCpfPaginado(string termo, int pagina, int tamanhoCorte,out int totalregistros);

    }
}
