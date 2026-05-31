using System.ComponentModel.DataAnnotations;

namespace LiberaMais.Models
{
    [Serializable]
    public class Util
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="O Nome é obrigatório")]
        public string Nome { get; set; }

        [Required(ErrorMessage ="O endereço eletronico é obrigatório")]
        public string Url { get; set; }

        public string? Descricao { get; set; }
    }
}
    