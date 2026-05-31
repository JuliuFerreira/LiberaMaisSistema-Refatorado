using LiberaMais.Helper;
using LiberaMais.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace LiberaMais.Models
{
    [Serializable]
    public class UsuarioModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Digite o nome do usuário")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Digite o login do usuário")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Digite o email do usuário")]
        [EmailAddress(ErrorMessage = "Digite um email válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Digite a senha do usuário")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "Selecione o perfil do usuário")]
        public PerfilEnum? Perfil { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public DateTime? DataAtualizacao { get; set; } = DateTime.Now;

        public virtual List<Venda>? Vendas { get; set; }




        public bool SenhaValida(string senha)
        {
            return Senha == senha.GerarHash();
        }
        public void setSenhaHash()
        {
            Senha = Senha.GerarHash();
        }

        public void setNovaSenha(string novaSenha)
        {
            Senha = novaSenha.GerarHash();
        }
        public string GerarNovaSenha()
        {
            string novaSenha = Guid.NewGuid().ToString().Substring(0, 8);
            Senha = novaSenha.GerarHash();
            return novaSenha;
        }


        
    }
}
