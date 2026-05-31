using LiberaMais.Filters;
using LiberaMais.Models;
using LiberaMais.Models.Enums;

namespace LiberaMais.Services
{
    public class PermissaoService
    {

        public bool UsuarioTemAcessoPromotora (UsuarioModel usuarioLogado, Promotora promotora)
        {
            if(usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                return true;
            }

            if(promotora == null)
            {
                return false;
            }
            return promotora.UsuarioId == usuarioLogado.Id;
        }

        public bool UsuarioTemAcessoPromotoraBanco(UsuarioModel usuarioLogado, PromotoraBanco promotoraBanco)
        {
         if(usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                return true;
            }

         if(promotoraBanco == null)
            {
                return false;
            }

            return promotoraBanco.Promotora.UsuarioId == usuarioLogado.Id;


        }

        public bool UsuarioTemAcessoCliente(UsuarioModel usuarioLogado, Cliente cliente)
        {
            if(usuarioLogado.Perfil == PerfilEnum.Admin)
            {
                return true;
            }

            if(cliente == null)
            {
                return false;
            }

            return cliente.UsuarioId == usuarioLogado.Id;
        }

    }
}
