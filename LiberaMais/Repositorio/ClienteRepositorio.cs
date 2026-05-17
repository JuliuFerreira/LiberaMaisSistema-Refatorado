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

        public Cliente BuscarClientePorId(long idcliente)
        {

            var result = _bancoContext.Clientes.FirstOrDefault(x => x.IdCliente == idcliente);


            return result;

        }

        public List<Cliente> ListarClientes(int usuarioId)
        {
            return _bancoContext.Clientes.Where(x => x.UsuarioId == usuarioId).ToList();
        }

        public List<Cliente> ListarTodosClientes()
        {
            return _bancoContext.Clientes.ToList();
        }

        public Cliente Adicionar(Cliente cliente)
        {
            _bancoContext.Clientes.Add(cliente);
            _bancoContext.SaveChanges();
            return cliente;
        }

        public Cliente Atualizar(Cliente cliente)
        {
            if (cliente == null)
            {
                throw new ArgumentNullException(nameof(cliente), "O cliente não pode ser nulo.");
            }

            _bancoContext.Clientes.Update(cliente);
            _bancoContext.SaveChanges();

            return cliente;
        }


        public bool Apagar(int idcliente)
        {
            Cliente cliente = BuscarClientePorId(idcliente);

            if (cliente == null) throw new System.Exception("Erro ao deletar o cliente!");

            _bancoContext.Clientes.Remove(cliente);
            _bancoContext.SaveChanges();

            return true;
        }

        public bool VerificarCpfExistente(string cpf)
        {
            // Faça uma consulta para buscar um cliente com o CPF fornecido
            var cliente = _bancoContext.Clientes.FirstOrDefault(x => x.Cpf == cpf);

            // Se o cliente não for nulo, significa que o CPF já existe no banco de dados
            return cliente != null;
        }



    }
}
