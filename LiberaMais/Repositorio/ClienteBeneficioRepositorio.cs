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

        public List<ClienteBeneficio> BuscarPorNomeCpfPaginado(string termo, int pagina, int tamanhoCorte, out int totalregistros)
        {
            // 1. Query base com os relacionamentos
            var query = _bancoContext.clienteBeneficio
                .Include(c => c.Cliente)
                .Include(c => c.Beneficio)
                    .ThenInclude(b => b.Orgaos)
                .AsQueryable();

            // 2. Aplica o filtro de busca se houver termo
            if (!string.IsNullOrWhiteSpace(termo))
            {
                string termoOriginal = termo.Trim();
                string termoLimpo = termo.Replace(".", "").Replace("-", "").Trim().ToLower();

                query = query.Where(c =>
                    (c.Cliente.Nome != null && c.Cliente.Nome.ToLower().Contains(termoLimpo)) ||
                    (c.Cliente.Cpf != null && (c.Cliente.Cpf.Contains(termoOriginal) || c.Cliente.Cpf.Contains(termoLimpo)))
                );
            }

            // 3. Traz para a memória e aplica o SEU agrupamento (Unicidade por CPF e Órgão)
            var todosFiltrados = query.ToList();

            var listaAgrupada = todosFiltrados
                .Where(cb => cb.Cliente != null && cb.Beneficio?.Orgaos != null)
                .GroupBy(cb => new { cb.Cliente.Cpf, cb.Beneficio.Orgaos.Id })
                .Select(grupo => grupo.First())
                .OrderBy(c => c.Cliente.Nome)
                .ToList(); // Lista final com TODOS os registros únicos possíveis para essa busca

            // 4. Guarda o total real de registros únicos
            totalregistros = listaAgrupada.Count;

            // 5. Faz o cálculo matemático do Skip de forma segura
            int registrosParaPular = (pagina - 1) * tamanhoCorte;

            // 6. Retorna apenas o pedaço (página) correto
            return listaAgrupada
                .Skip(registrosParaPular)
                .Take(tamanhoCorte)
                .ToList();
        }
    }
}
