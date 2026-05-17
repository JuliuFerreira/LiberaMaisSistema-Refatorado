using System.ComponentModel.DataAnnotations;

namespace LiberaMais.Models
{
    public class AlterarSenha
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Digite sua senha atual")]
        public string SenhaAtual { get; set; }

        [Required(ErrorMessage = "Digite a sua nova senha")]
        public string NovaSenha { get; set; }

        [Required(ErrorMessage = "Confirme a nova senha")]
        [Compare("NovaSenha", ErrorMessage ="A senha digitada é diferente da nova senha!")]
        public string ConfirmarNovaSenha { get; set; } 

    }
}
