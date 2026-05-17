using System.ComponentModel.DataAnnotations;

namespace LiberaMais.Models
{
    public class RedefinirSenha
    {
        [Required(ErrorMessage ="Insira seu login!")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Insira seu email!")]
        public string Email { get; set; }
    }
}
