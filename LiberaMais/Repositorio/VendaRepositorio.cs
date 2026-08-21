
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

        public Venda BuscarVendaPorId(int id)
        {
            var result = _bancoContext.Vendas
            .Include(v => v.Usuario)
            //.Include(v => v.Banco)
            .Include(v => v.Promotora)
            .Include(v => v.Cliente)
            .Include(v => v.Banco)
            .Include(v => v.ClienteBeneficio)
            .ThenInclude(v => v.Beneficio)
            .ThenInclude(v => v.Orgaos)
            .FirstOrDefault(v => v.Id == id);

            return result;
        }


        public List<Venda> ListarTodasVendas()
        {
            return _bancoContext.Vendas
            .Include(v => v.Usuario)
            //.Include(v => v.Banco)
            .Include(v => v.Promotora)
            .Include(v => v.Banco)
            .Include(v => v.Cliente)
            .Include(v => v.ClienteBeneficio)
            .ThenInclude(v => v.Beneficio)
            .ThenInclude(v => v.Orgaos)
            .ToList();
        }


        public Venda Adicionar(Venda venda)
        {
            _bancoContext.Vendas.Add(venda);
            _bancoContext.SaveChanges();
            return venda;
        }

        public Venda Atualizar(Venda venda)
        {           
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

        public List<Venda> ListarVendasPorUsuario(int usuarioId)
        {
            return _bancoContext.Vendas
                .Include(v => v.Usuario)
            //.Include(v => v.Banco)
            .Include(v => v.Promotora)
            .Include(v => v.Cliente)
            .Include(v => v.Banco)
            .Include(v => v.ClienteBeneficio)
            .ThenInclude(v => v.Beneficio)
            .ThenInclude(v => v.Orgaos)
            .Where(v => v.UsuarioId == usuarioId)
            .ToList();
        }

        public List<Venda> BuscarVendasPagasPorPeriodo(int mes, int ano)
        {
            return _bancoContext.Venda
            .Include(v => v.Cliente)
            .Include(v => v.Promotora)
            .Include(v => v.Banco)
            .Where(v => v.DataPgtoComissao.HasValue &&
            v.DataPgtoComissao.Value.Month == mes &&
            v.DataPgtoComissao.Value.Year == ano)
            .ToList();  
        }

        public List<Venda> BuscarCompleto(string termo, int pagina, int tamanhoCorte, out int totalRegistros)
        {
            var query = _bancoContext.Venda.Include(v => v.Usuario)
                .Include(v => v.Banco)
                .Include(v => v.Cliente)
                .Include(v => v.Promotora)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                string termoLimpo = termo.Replace(".", "").Replace("-", "").Trim().ToLower();
                query = query.Where(v => (v.Cliente.Nome != null && v.Cliente.Nome.Contains(termoLimpo)) ||
                (v.Cliente.Cpf != null && v.Cliente.Cpf.Replace(".", "").Replace("-", "").Contains(termoLimpo)) ||
                (v.Banco.Nome != null && v.Banco.Nome.ToLower().Contains(termoLimpo)) ||
                (v.Promotora.Nome != null && v.Promotora.Nome.ToLower().Contains(termoLimpo)));
            }

            totalRegistros = query.Count();
            return query.OrderBy(v => v.DataCadastro)
                .Skip((pagina - 1) * tamanhoCorte)
                .Take(tamanhoCorte)
                .ToList();
        }

        public void SalvarAlteracoes()
        {
            _bancoContext.SaveChanges();
        }
    }

}