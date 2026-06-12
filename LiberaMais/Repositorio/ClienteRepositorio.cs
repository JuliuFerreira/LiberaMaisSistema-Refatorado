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

            Cliente cliente = BuscarClientePorId(id);
            if(cliente == null)
            {
                return false;
            }
            _bancoContext.Clientes.Remove(cliente);
            _bancoContext.SaveChanges();
            return true;
        }

        public List<Cliente> BuscarClientesPorUsuarioId(int usuarioId)
        {
            return _bancoContext.Clientes
                .Include(c => c.Usuario)
                .Where(c => c.UsuarioId == usuarioId) 
                .ToList();
        }

        public Cliente BuscarCompleto(int id)
        {
         return _bancoContext.Clientes
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

        //public bool VerificarCpfExistente(string cpf)
        //{
        //    // Faça uma consulta para buscar um cliente com o CPF fornecido
        //    var cliente = _bancoContext.Clientes.FirstOrDefault(x => x.Cpf == cpf);

        //    // Se o cliente não for nulo, significa que o CPF já existe no banco de dados
        //    return cliente != null;
        //}



    }
}
