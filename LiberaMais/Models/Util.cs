using System.ComponentModel.DataAnnotations;

namespace LiberaMais.Models
{
    [Serializable]
    public class Util
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório")]
        public string Nome { get; set; }

        
        //[Required(ErrorMessage = "O endereço eletronico é obrigatório")]
        //[RegularExpression(@"^(https?:\/\/)?(www\.)[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+(\.[a-zA-Z]{2,})?$",
        //ErrorMessage = "Por favor, insira uma URL válida (ex: ://site.com ou ://site.com.br)")]
        public string Url { get; set; }

        public string? Descricao { get; set; }
    }
}
