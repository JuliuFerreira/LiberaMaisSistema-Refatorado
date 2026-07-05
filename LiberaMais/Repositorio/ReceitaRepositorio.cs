using LiberaMais.Controllers;
using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Repositorio
{
    public class ReceitaRepositorio : IReceitaRepositorio
    {
        private readonly BancoContext _bancoContext;
        public ReceitaRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public Receita BuscarReceitaPorId(int id)
        {
            var result = _bancoContext.receitas.FirstOrDefault(x => x.Id == id);

            return result;
        }

        public List<Receita> ListarReceitas(int idFinanca)
        {
            return _bancoContext.receitas
                           .Include(r => r.Promotora) // <-- Crucial para não vir nulo na listagem!
                           .Where(x => x.FinancaId == idFinanca)
                           .ToList();
        }

        public Receita Adicionar(Receita receita)
        {
            _bancoContext.receitas.Add(receita);
            _bancoContext.SaveChanges();
            return receita;
        }

        public Receita Atualizar(Receita receita)
        {
            _bancoContext.receitas.Update(receita);
            _bancoContext.SaveChanges();
            return receita;
        }

        public bool Apagar(int id)
        {
            Receita receita = BuscarReceitaPorId(id);

            if (receita == null) throw new System.Exception("Erro ao excluir a receita!");

            _bancoContext.receitas.Remove(receita);
            _bancoContext.SaveChanges();
            return true;
        }

       

        
    }
}
