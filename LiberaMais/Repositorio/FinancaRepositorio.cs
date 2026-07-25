using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Repositorio
{
    public class FinancaRepositorio : IFinancaRepositorio
    {
        private readonly BancoContext _bancoContext;

        public FinancaRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }



        public List<Financa> BuscarTodos(int usuarioId, int mes, int ano)
        {
            return _bancoContext.financas
                .Include(f => f.Promotora)
                .Where(f => f.UsuarioId == usuarioId && f.Mes == mes && f.Ano == ano)
                .OrderBy(f => f.Data)
                .ToList();           
        }

        public Financa Adicionar(Financa financa)
        {

            _bancoContext.financas.Add(financa);
            _bancoContext.SaveChanges();
            return financa;
        }

        public Financa Atualizar(Financa financa)
        {
            _bancoContext.financas.Update(financa);
            _bancoContext.SaveChanges();
            return financa;
        }

        public bool Apagar(int id)
        {
            Financa financa = BuscarPorId(id);

            if (financa == null) throw new System.Exception("Erro ao excluir Finança");

            _bancoContext.financas.Remove(financa);
            _bancoContext.SaveChanges();
            return true;
        }

        public Financa BuscarPorId(int id)
        {
            return _bancoContext.financas
                .Include(f => f.Promotora)
            .FirstOrDefault(f => f.Id == id);
        }

        public List<Financa> ListarPorPeriodo(int mes, int ano, int? usuarioId)
        {
            // Adicionamos o .Include logo aqui, no início da consulta
            var query = _bancoContext.financas
                .Include(f => f.Promotora)
                .AsQueryable();

            // Filtramos pelo mês e ano
            query = query.Where(x => x.Mes == mes && x.Ano == ano);

            // Se um usuário foi selecionado, filtramos por ele também
            if (usuarioId.HasValue)
            {
                query = query.Where(x => x.UsuarioId == usuarioId);
            }

            // Retorna a lista final com os dados da promotora carregados
            return query.ToList();
        }
    }
}
