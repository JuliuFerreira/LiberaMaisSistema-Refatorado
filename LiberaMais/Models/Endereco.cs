using System.ComponentModel.DataAnnotations;

namespace LiberaMais.Models
{
    public class Endereco
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "O Cep é obrigatório")]
        public string Cep { get; set; }

        [Required(ErrorMessage = "O nome da rua é obrigatório")]
        public string Rua { get; set; }

        [Required(ErrorMessage = "O número da casa é obrigatório")]
        public string Numero { get; set; }

        [Required(ErrorMessage = "O Bairro é obrigatório")]
        public string Bairro { get; set; }

        [Required(ErrorMessage = "A Cidade é obrigatória")]
        public string Cidade { get; set; }

        [Required(ErrorMessage = "O Estado é obrigatório")]
        public string Estado { get; set; }

        public int ClienteId { get; set; }

        public Cliente? Cliente { get; set; }       

           
    }
}
