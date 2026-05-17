using System.ComponentModel.DataAnnotations;

namespace LiberaMais.Models
{
    [Serializable]
    public class LoginModel
    {
        [Required(ErrorMessage = "Digite o seu login!")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Digite a sua senha!")]
        public string Senha { get; set; }
    }
}
