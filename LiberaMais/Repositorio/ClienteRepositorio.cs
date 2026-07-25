using LiberaMais.Controllers;
using LiberaMais.Data;
using LiberaMais.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace LiberaMais.Repositorio
{
    public class ClienteRepositorio : IClienteRepositorio
    {
        private readonly BancoContext _bancoContext;
        public ClienteRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public Cliente BuscarClientePorId(int id)
        {
            return _bancoContext.Clientes

                .Include(c => c.Endereco)

                .Include(c => c.ClienteBeneficios)

                    .ThenInclude(cb => cb.Beneficio)

                        .ThenInclude(b => b.Orgaos)

                .FirstOrDefault(c => c.Id == id);
        }

        public List<Cliente> ListarTodosClientes()
        {
            return _bancoContext.Clientes
            .Include(c => c.Endereco)
            .Include (c => c.Usuario)
            .Include(c => c.ClienteBeneficios)
            .ToList();
        }

        public Cliente Adicionar(Cliente cliente)  /*BancoContext bancoContext*/
        {
            _bancoContext.Clientes.Add(cliente);
            _bancoContext.SaveChanges();
            return cliente;
        }

        public Cliente Atualizar(Cliente cliente)
        {

            _bancoContext.Clientes.Update(cliente);
            _bancoContext.SaveChanges();
            return cliente;
        }


        public bool Apagar(int id)
        {
            // O Include garante que tragamos o endereço e benefícios mapeados para poder excluí-los juntos
            var clienteDb = _bancoContext.Clientes
                .Include(c => c.Endereco)
                .Include(c => c.ClienteBeneficios)
                .FirstOrDefault(c => c.Id == id);

            if (clienteDb == null) return false;

            // Remove o endereço vinculado se ele existir
            if (clienteDb.Endereco != null)
            {
                _bancoContext.Enderecos.Remove(clienteDb.Endereco);
            }

            // Remove a lista de benefícios vinculados se houver
            if (clienteDb.ClienteBeneficios != null && clienteDb.ClienteBeneficios.Any())
            {
                _bancoContext.clienteBeneficio.RemoveRange(clienteDb.ClienteBeneficios);
            }

            // Por fim, remove o cliente
            _bancoContext.Clientes.Remove(clienteDb);

            _bancoContext.SaveChanges();
            return true;
        }
        public List<Cliente> BuscarClientesPorUsuarioId(int usuarioId)
        {
            return _bancoContext.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.ClienteBeneficios)
                .Include(c => c.Endereco)

                .Where(c => c.UsuarioId == usuarioId)

                .ToList();
        }

        public Cliente BuscarCompleto(int id)
        {
         return _bancoContext.Clientes
        .Include(c => c.Usuario)
        .Include(c => c.Endereco)
        .Include(c => c.ClienteBeneficios)
            .ThenInclude(cb => cb.Beneficio)
                .ThenInclude(b => b.Orgaos)
        .FirstOrDefault(c => c.Id == id);
        }

        public Cliente BuscarPorCpf(string cpf)
        {
            return _bancoContext.Clientes
                 .FirstOrDefault(c => c.Cpf == cpf);
        }

        public List<Cliente> BuscarPorNomeOuCpfPaginado(string termo, int pagina, int tamanhoCorte, out int totalRegistros)
        {
            // O .Include garante que a tabela de Usuários seja consultada junta (Eager Loading)
            var query = _bancoContext.Clientes.Include(c => c.Usuario).AsQueryable();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                string termoLimpo = termo.Replace(".", "").Replace("-", "").Trim().ToLower();
                query = query.Where(c => (c.Nome != null && c.Nome.ToLower().Contains(termoLimpo)) ||
                                         (c.Cpf != null && c.Cpf.Replace(".", "").Replace("-", "").Contains(termoLimpo)));
            }

            totalRegistros = query.Count();

            return query.OrderBy(c => c.Nome)
                        .Skip((pagina - 1) * tamanhoCorte)
                        .Take(tamanhoCorte)
                        .ToList();
        }

        //public Cliente BuscarPorCpfeNome(string cpf, string nome)
        //{
        //    return _bancoContext.Clientes
        //        .FirstOrDefault(c => c.Cpf == cpf)

        //}

        //public bool VerificarCpfExistente(string cpf)
        //{
        //    // Faça uma consulta para buscar um cliente com o CPF fornecido
        //    var cliente = _bancoContext.Clientes.FirstOrDefault(x => x.Cpf == cpf);

        //    // Se o cliente não for nulo, significa que o CPF já existe no banco de dados
        //    return cliente != null;
        //}



    }
}
