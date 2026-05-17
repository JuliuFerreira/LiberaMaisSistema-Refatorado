using LiberaMais.Models;

namespace LiberaMais.Repositorio
{
    public interface IUsuarioRepositorio
    {
        UsuarioModel BuscarPorLogin(string login);

        UsuarioModel BuscarPorEmailELogin(string email, string login);

        UsuarioModel BuscarUsuarioPorId(int id);

        List<UsuarioModel> BuscarTodos();

        UsuarioModel BuscarPorId(int id);

        UsuarioModel Adicionar(UsuarioModel usuario);

        UsuarioModel Atualizar(UsuarioModel usuario);

        UsuarioModel AlterarSenha(AlterarSenha alterarSenha);

        List<UsuarioModel> ListarTodosUsuarios();

        bool Apagar(int id);

    }
}
