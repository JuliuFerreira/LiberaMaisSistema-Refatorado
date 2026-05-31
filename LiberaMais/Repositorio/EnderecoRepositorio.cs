using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Repositorio
{
    public class EnderecoRepositorio : IEnderecoRepositorio
    {

        private readonly BancoContext _bancoContext;

        public EnderecoRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }
        public Endereco Adicionar(Endereco endereco)
        {
            _bancoContext.Enderecos.Add(endereco);
            _bancoContext.SaveChanges();
            return (endereco);
        }

        public Endereco Atualizar(Endereco endereco)
        {
            _bancoContext.Enderecos.Update(endereco);
            _bancoContext.SaveChanges();
            return (endereco);
        }

        public Endereco BuscarPorId(int id)
        {
            return _bancoContext.Enderecos
                 .Include(c => c.Cliente)
                 .FirstOrDefault(e => e.Id == id);
        }

        public List<Endereco> ListarTodos()
        {
            return _bancoContext.Enderecos .ToList();
        }

        public bool Apagar(int id)
        {
            Endereco endereco = BuscarPorId(id);
            if(endereco == null)
            {
                return false;
            }
            _bancoContext.Remove(endereco);
            _bancoContext.SaveChanges();
            return true;
        }


    }
}
