using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Repositorio
{
    public class ClienteBeneficioRepositorio: IClienteBeneficioRepositorio
    {
        private readonly BancoContext _bancoContext;

        public ClienteBeneficioRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public List<ClienteBeneficio> ListarTodos()
        {
            return _bancoContext.clienteBeneficio
                .Include(b => b.Cliente)
                .Include(b => b.Beneficio)
                .ThenInclude(b => b.Orgaos) // <- Carrega em relacionamento dentro de outro relacionamento.
                .ToList();
        }


        public ClienteBeneficio Adicionar(ClienteBeneficio clienteBeneficio)
        {
            _bancoContext.clienteBeneficio.Add(clienteBeneficio);
            _bancoContext.SaveChanges();
            return clienteBeneficio;
        }

        public bool Apagar(int id)
        {
            ClienteBeneficio clienteBeneficio = BuscarPorId(id);

            if(clienteBeneficio == null)
            {
                return false;
            }

            _bancoContext.clienteBeneficio.Remove(clienteBeneficio);
            _bancoContext.SaveChanges();
            return true;
        }

        public ClienteBeneficio Atualizar(ClienteBeneficio clienteBeneficio)
        {
            _bancoContext.clienteBeneficio.Update(clienteBeneficio);
            _bancoContext.SaveChanges();
            return clienteBeneficio;
        }

        public ClienteBeneficio BuscarPorId(int id)
        {
            return _bancoContext.clienteBeneficio
                .Include(b => b.Cliente)
                .Include(b => b.Beneficio)
                .ThenInclude(b => b.Orgaos)
                .FirstOrDefault(b => b.Id == id);
        }

        public List<ClienteBeneficio> ListarBeneficiosPorCliente(int clienteId)
        {
            return _bancoContext.clienteBeneficio
                .Include(cb => cb.Beneficio)
                .ThenInclude(b => b.Orgaos)
                .Where(cb => cb.ClienteId == clienteId)
                .ToList();
        }

        public void ApagarPorClienteId(int clienteId)
        {
            // 1. Busca todos os benefícios daquele cliente
            var beneficios = _bancoContext.clienteBeneficio.Where(b => b.ClienteId == clienteId).ToList();

            // 2. Se houver algum, deleta todos de uma vez só
            if (beneficios.Any())
            {
                _bancoContext.clienteBeneficio.RemoveRange(beneficios);
                _bancoContext.SaveChanges();
            }
        }
    }
}
