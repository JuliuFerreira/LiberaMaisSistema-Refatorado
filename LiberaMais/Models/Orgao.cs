using System.ComponentModel.DataAnnotations;

namespace LiberaMais.Models
{
    [Serializable]
    public class Orgao
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "É obrigatório o nome do orgão.")]
        public string Nome { get; set; }
    }
}
