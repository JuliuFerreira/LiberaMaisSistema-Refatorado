using LiberaMais.Data;
using LiberaMais.Models;
using Microsoft.EntityFrameworkCore;

namespace LiberaMais.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly BancoContext _bancoContext;

        public UsuarioRepositorio(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        public UsuarioModel Adicionar(UsuarioModel usuario)
        {
            usuario.DataCadastro = DateTime.Now;
            usuario.setSenhaHash();
            _bancoContext.Usuarios.Add(usuario);
            _bancoContext.SaveChanges();
            return usuario;
        }

        public UsuarioModel BuscarUsuarioPorId(int id)
        {
            var result = _bancoContext.Usuarios.FirstOrDefault(x => x.Id == id);

            return result;
        }

        public UsuarioModel BuscarPorLogin(string login)
        {
            return _bancoContext.Usuarios.FirstOrDefault(x => x.Login.Trim().ToLower() == login.Trim().ToLower());
        }

        public UsuarioModel BuscarPorEmailELogin(string email, string login)
        {
            return _bancoContext.Usuarios.FirstOrDefault(x => x.Login.ToUpper() == login.ToUpper() && x.Email.ToUpper() == email.ToUpper());

        }

        public bool Apagar(int id)
        {
            UsuarioModel usuario = BuscarPorId(id);

            if (usuario == null) throw new System.Exception("Houve um erro ao excluir o usuário!");

            _bancoContext.Usuarios.Remove(usuario);
            _bancoContext.SaveChanges();
            return true;
        }

        public UsuarioModel Atualizar(UsuarioModel usuario)
        {
            UsuarioModel usuarioDB = BuscarPorId(usuario.Id);

            if (usuarioDB == null) throw new Exception("Houve um erro ao atualizar o usuário!");

            usuarioDB.Nome = usuario.Nome;
            usuarioDB.Login = usuario.Login;
            usuarioDB.Email = usuario.Email;
            usuarioDB.Perfil = usuario.Perfil;
            usuarioDB.DataAtualizacao = DateTime.Now;

            _bancoContext.Usuarios.Update(usuarioDB);
            _bancoContext.SaveChanges();

            return usuarioDB;
        }

        public UsuarioModel AlterarSenha(AlterarSenha alterarSenha)
        {
            UsuarioModel usuarioDB = BuscarPorId(alterarSenha.Id);

            if (usuarioDB == null) throw new Exception("Ocorreu um erro na atualização da senha, o usuário não foi encontrado.");

            if (!usuarioDB.SenhaValida(alterarSenha.SenhaAtual)) throw new Exception("Senha atual não confere!");

            if (usuarioDB.SenhaValida(alterarSenha.NovaSenha)) throw new Exception("A nova senha deve ser diferente da senha atual");

            usuarioDB.setNovaSenha(alterarSenha.NovaSenha);
            usuarioDB.DataAtualizacao = DateTime.Now;

            _bancoContext.Usuarios.Update(usuarioDB);
            _bancoContext.SaveChanges();

            return usuarioDB;
        }

        public UsuarioModel BuscarPorId(int id)
        {
            return _bancoContext.Usuarios.FirstOrDefault(j => j.Id == id);
        }        

        public List<UsuarioModel> BuscarTodos()
        {
            return _bancoContext.Usuarios
                .Include(x => x.Vendas)
                .ToList();
        }

        public List<UsuarioModel> ListarTodosUsuarios()
        {
            return _bancoContext.Usuarios.ToList();
        }

        public UsuarioModel BuscarPorNome(string nome)
        {
            return _bancoContext.Usuarios
                .FirstOrDefault(u => u.Nome.Trim().ToLower() == nome.Trim().ToLower());
        }
    }
}
