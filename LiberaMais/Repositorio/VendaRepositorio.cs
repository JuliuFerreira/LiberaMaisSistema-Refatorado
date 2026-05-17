
using LiberaMais.Data;
using LiberaMais.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using LiberaMais.Models.Enums;

namespace LiberaMais.Repositorio
{
    public class VendaRepositorio : IVendaRepositorio
    {
        private readonly BancoContext _bancoContext;
        public VendaRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public Venda BuscarVendaPorId(long id)
        {
            var result = _bancoContext.Vendas.FirstOrDefault(j => j.Id == id);

            return result;
        }

        public List<Venda> ListarVenda(int usuarioId)
        {
            
            return _bancoContext.Vendas.Where(x => x.UsuarioId == usuarioId).ToList();
        }

        public List<Venda> ListarTodasVendas()
        {
            return _bancoContext.Vendas.ToList();
        }


        public Venda Adicionar(Venda venda)
        {
            //_bancoContext.Contato.Add(cliente.Contato);
            // _bancoContext.Endereco.Add(cliente.Endereco);
            _bancoContext.Vendas.Add(venda);
            _bancoContext.SaveChanges();
            return venda;
        }

        public Venda Atualizar(Venda venda)
        {
            if (venda == null)
                throw new Exception("Erro ao atualizar vendas!");

            _bancoContext.Vendas.Update(venda);
            _bancoContext.SaveChanges();

            return venda;
        }

        public bool Apagar(int id)
        {
            Venda venda = BuscarVendaPorId(id);

            if (venda == null) throw new System.Exception("Erro ao excluir a venda!");

            _bancoContext.Vendas.Remove(venda);
            _bancoContext.SaveChanges();
            return true;
        }


        public bool VerificarCpfExistente(string cpf)
        {
            // Faça uma consulta para buscar um cliente com o CPF fornecido
            var venda = _bancoContext.Vendas.FirstOrDefault(j => j.Cpf == cpf);

            // Se o cliente não for nulo, significa que o CPF já existe no banco de dados
            return venda != null;
        }

        public List<Venda> ListarVendasPorPeriodoEUsuario(DateTime startDate, DateTime endDate, List<int> userIds, string searchString)
        {
            var query = _bancoContext.Vendas.AsQueryable();

            // Aplicar filtro por período
            query = query.Where(v => v.Data >= startDate && v.Data <= endDate);

            // Aplicar filtro por status "PAGO"
            query = query.Where(v => (int)v.Status == (int)StatusEnum.PAGO);

            // Aplicar filtro por nome do usuário (caso searchString não seja nulo ou vazio)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v => v.UsuarioNome.Contains(searchString));
            }

            // Aplicar filtro por lista de IDs de usuário
            if (userIds != null && userIds.Any())
            {
                query = query.Where(v => userIds.Contains(v.UsuarioId));
            }

            return query.ToList();
        }

        public List<Venda> ListarVendasApenasPagos()
        {
            int statusPago = (int)StatusEnum.PAGO; //  Converte o enum para int

            return _bancoContext.Vendas.Where(v => v.Status == statusPago).ToList(); // Compara o valor inteiro


        }
    }

}