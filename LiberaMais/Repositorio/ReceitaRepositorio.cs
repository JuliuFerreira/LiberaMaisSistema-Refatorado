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
            var listarReceitas = _bancoContext.receitas.Where(j => j.FinancaId == idFinanca).ToList();

            if (listarReceitas.Count == 0)
            {
                var receita = new Receita()
                {
                    FinancaId = idFinanca,
                };

                listarReceitas.Add(receita);
            }

            return listarReceitas;
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
